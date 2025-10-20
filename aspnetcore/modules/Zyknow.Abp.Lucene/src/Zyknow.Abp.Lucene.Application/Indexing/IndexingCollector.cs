using System.Collections;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using Zyknow.Abp.Lucene.Services;

namespace Zyknow.Abp.Lucene.Indexing;

/// <summary>
/// 在单个作用域/UnitOfWork 内聚合需要写入 Lucene 的实体变更，
/// 并在事务提交后一次性批处理执行，降低 IndexWriter 打开/提交频次。
/// </summary>
public interface IIndexingCollector
{
    IReadOnlyDictionary<Type, Dictionary<string, object>> Upserts { get; }
    IReadOnlyDictionary<Type, HashSet<string>> Deletes { get; }
    void Upsert<T>(T entity, string id);
    void Delete<T>(string id);

    /// <summary>
    /// 确保仅注册一次提交后的批处理回调。
    /// </summary>
    void RegisterOnCompleted(IUnitOfWork uow, LuceneIndexManager indexer);

    Task ProcessImmediatelyAsync(LuceneIndexManager indexer);
}

public class IndexingCollector : IIndexingCollector, IScopedDependency
{
    private readonly Dictionary<Type, HashSet<string>> _deletes = new();
    private readonly Dictionary<Type, Dictionary<string, object>> _upserts = new();
    private bool _registered;

    public IReadOnlyDictionary<Type, Dictionary<string, object>> Upserts => _upserts;
    public IReadOnlyDictionary<Type, HashSet<string>> Deletes => _deletes;

    public void Upsert<T>(T entity, string id)
    {
        var type = typeof(T);
        if (!_upserts.TryGetValue(type, out var map))
        {
            map = new Dictionary<string, object>();
            _upserts[type] = map;
        }

        map[id] = entity!;
    }

    public void Delete<T>(string id)
    {
        var type = typeof(T);
        if (!_deletes.TryGetValue(type, out var set))
        {
            set = new HashSet<string>(StringComparer.Ordinal);
            _deletes[type] = set;
        }

        set.Add(id);
    }

    public void RegisterOnCompleted(IUnitOfWork uow, LuceneIndexManager indexer)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        uow.OnCompleted(async () =>
        {
            await FlushAsync(indexer);
            Reset();
        });
    }

    public Task ProcessImmediatelyAsync(LuceneIndexManager indexer)
    {
        return FlushAsync(indexer);
    }

    private async Task FlushAsync(LuceneIndexManager indexer)
    {
        // 删除优先：避免同一事务内先 upsert 后删除导致最终仍被索引
        foreach (var (type, ids) in _deletes)
        {
            // 从 upserts 中移除将被删除的主键
            if (_upserts.TryGetValue(type, out var map))
            {
                foreach (var id in ids)
                {
                    map.Remove(id);
                }
            }
        }

        // 批量 upsert
        foreach (var (type, map) in _upserts)
        {
            if (map.Count == 0)
            {
                continue;
            }

            var indexRange = typeof(LuceneIndexManager).GetMethod(nameof(LuceneIndexManager.IndexRangeAsync))!;
            var generic = indexRange.MakeGenericMethod(type);

            // 构造 List<T> 参数
            var listType = typeof(List<>).MakeGenericType(type);
            var list = Activator.CreateInstance(listType)!;
            var ilist = (IList)list;
            foreach (var obj in map.Values)
            {
                ilist.Add(obj);
            }

            // replace:false 表示增量更新
            await (Task)generic.Invoke(indexer, [list, false])!;
        }

        // 批量删除
        foreach (var (type, ids) in _deletes)
        {
            var deleteRange = typeof(LuceneIndexManager).GetMethod(nameof(LuceneIndexManager.DeleteRangeAsync))!;
            var gdeleteRange = deleteRange.MakeGenericMethod(type);
            // 构造 List<object> 参数
            var list = new List<object>(ids);
            await (Task)gdeleteRange.Invoke(indexer, [list])!;
        }
    }

    private void Reset()
    {
        _upserts.Clear();
        _deletes.Clear();
        _registered = false;
    }
}