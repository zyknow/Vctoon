using System.Linq.Expressions;
using System.Reflection;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Zyknow.Abp.Lucene.Descriptors;
using Zyknow.Abp.Lucene.Indexing;
using Zyknow.Abp.Lucene.Options;

namespace Zyknow.Abp.Lucene.Services;

/// <summary>
/// 索引同步服务：从数据库批量读取实体并补齐索引，同时删除索引中多余的文档，保证与数据库一致。
/// </summary>
public class LuceneIndexSynchronizer(
    LuceneIndexManager indexManager,
    IOptions<LuceneOptions> options,
    ICurrentTenant currentTenant,
    IAsyncQueryableExecuter asyncExecuter,
    ILogger<LuceneIndexSynchronizer> logger)
    : ITransientDependency
{
    /// <summary>
    /// 泛型主键版本：按实体类型进行全量/增量同步，兼容非 Guid 主键。
    /// </summary>
    public async Task SyncAllAsync<T, TKey>(IRepository<T, TKey> repository, int batchSize = 1000,
        bool deleteMissing = true, CancellationToken ct = default)
        where T : class, IEntity<TKey>
        where TKey : notnull
    {
        var descriptor = GetDescriptor(typeof(T));
        var hasValueSelectors = descriptor.Fields.Any(f => f.ValueSelector != null);

        // 自动根据描述器进行字段投影（仅当无 ValueSelector 时）
        if (!hasValueSelectors)
        {
            var propNames = descriptor.Fields
                .Where(f => f.Selector != null)
                .Select(f => InferNameLocal(f.Selector!))
                .Distinct()
                .ToList();

            logger.LogDebug("Lucene sync using projection for {Entity} (Index:{IndexName}) Fields:{Fields}",
                typeof(T).FullName, descriptor.IndexName, string.Join(",", propNames));
            var skip = 0;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var query = await repository.GetQueryableAsync();
                var selector = BuildProjectionSelector<T>(propNames, descriptor.IdFieldName);
                var projected = query.Select(selector).Skip(skip).Take(batchSize);
                var rows = await asyncExecuter.ToListAsync(projected, ct);
                if (rows.Count == 0)
                {
                    break;
                }

                var docs = new List<Document>(rows.Count);
                for (var r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];
                    var values = new Dictionary<string, string>(propNames.Count + 1)
                    {
                        [descriptor.IdFieldName] = row.Id?.ToString() ?? string.Empty
                    };
                    for (var i = 0; i < propNames.Count; i++)
                    {
                        var val = GetV(row, i)?.ToString() ?? string.Empty;
                        values[propNames[i]] = val;
                    }

                    var doc = LuceneDocumentFactory.CreateDocument(values, descriptor);
                    docs.Add(doc);
                }

                await indexManager.IndexRangeDocumentsAsync(descriptor, docs, false);
                skip += rows.Count;
            }
        }
        else
        {
            var valueFields = descriptor.Fields.Where(f => f.ValueSelector != null).Select(f => f.Name).Distinct()
                .ToList();
            logger.LogInformation(
                "Lucene sync fallback to entity load for {Entity} (Index:{IndexName}). ValueFields:{Fields}",
                typeof(T).FullName, descriptor.IndexName, string.Join(",", valueFields));

            // 回退：存在 ValueSelector 时加载完整实体，保证正确性
            var depends =
                new HashSet<string>(descriptor.Fields.SelectMany(f => f.Depends).Append(descriptor.IdFieldName));
            var skip = 0;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var query = await repository.GetQueryableAsync();
                var batchQuery = query.Skip(skip).Take(batchSize);
                var batch = await asyncExecuter.ToListAsync(batchQuery, ct);
                if (batch.Count == 0)
                {
                    break;
                }

                await indexManager.IndexRangeAsync(batch, false);
                skip += batch.Count;
            }
        }

        if (!deleteMissing)
        {
            return;
        }

        // 对账删除：索引 Id - 数据库 Id 的差集（以字符串比较）
        var dbIdQuery = (await repository.GetQueryableAsync()).Select(x => x.Id);
        var dbIds = await asyncExecuter.ToListAsync(dbIdQuery, ct);
        var dbIdStrings = new HashSet<string>(dbIds.Select(x => x?.ToString() ?? string.Empty));

        var indexIds = await ListIndexedIdsAsync(descriptor);
        var staleIds = indexIds.Where(id => !dbIdStrings.Contains(id)).Cast<object>().ToList();
        if (staleIds.Count > 0)
        {
            await indexManager.DeleteRangeAsync<T>(staleIds);
        }
    }

    private static object? GetV(IndexShape row, int idx)
    {
        return idx switch
        {
            0 => row.V0,
            1 => row.V1,
            2 => row.V2,
            3 => row.V3,
            4 => row.V4,
            5 => row.V5,
            6 => row.V6,
            7 => row.V7,
            8 => row.V8,
            9 => row.V9,
            10 => row.V10,
            11 => row.V11,
            12 => row.V12,
            13 => row.V13,
            14 => row.V14,
            15 => row.V15,
            _ => null
        };
    }

    private static Expression<Func<T, IndexShape>> BuildProjectionSelector<T>(
        IReadOnlyList<string> propNames, string idFieldName)
    {
        var p = Expression.Parameter(typeof(T), "x");
        var bindings = new List<MemberBinding>();

        var idProp = typeof(T).GetProperty(idFieldName, BindingFlags.Public | BindingFlags.Instance);
        if (idProp != null)
        {
            var idExpr = Expression.Property(p, idProp);
            var boxId = Expression.Convert(idExpr, typeof(object));
            var idBind =
                Expression.Bind(typeof(IndexShape).GetProperty(nameof(IndexShape.Id))!, boxId);
            bindings.Add(idBind);
        }

        var count = Math.Min(propNames.Count, 16);
        for (var i = 0; i < count; i++)
        {
            var name = propNames[i];
            var prop = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                continue;
            }

            var valExpr = Expression.Property(p, prop);
            var box = Expression.Convert(valExpr, typeof(object));
            var targetProp = typeof(IndexShape).GetProperty($"V{i}")!;
            var bind = Expression.Bind(targetProp, box);
            bindings.Add(bind);
        }

        var body = Expression.MemberInit(
            Expression.New(typeof(IndexShape)), bindings);
        return Expression.Lambda<Func<T, IndexShape>>(body, p);
    }

    /// <summary>
    /// Guid 主键版本（兼容旧调用），内部委托到泛型主键版本。
    /// </summary>
    public Task SyncAllAsync<T>(IRepository<T, Guid> repository, int batchSize = 1000, bool deleteMissing = true,
        CancellationToken ct = default)
        where T : class, IEntity<Guid>
    {
        return SyncAllAsync<T, Guid>(repository, batchSize, deleteMissing, ct);
    }

    /// <summary>
    /// 遍历当前租户下所有已注册的实体描述符并执行同步（泛型主键）。
    /// </summary>
    public async Task SyncTenantAsync(Guid? tenantId = null, Func<Type, object>? repositoryFactory = null,
        int batchSize = 1000, bool deleteMissing = true, CancellationToken ct = default)
    {
        using var change = currentTenant.Change(tenantId);
        foreach (var kv in options.Value.Descriptors)
        {
            var entityType = kv.Key;
            var keyType = ResolveKeyType(entityType);
            if (keyType is null)
            {
                continue;
            }

            var repoObj = repositoryFactory?.Invoke(entityType);
            if (repoObj == null)
            {
                continue; // 必须由调用方提供 IRepository<TEntity, TKey>
            }

            var method = typeof(LuceneIndexSynchronizer)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => m.Name == nameof(SyncAllAsync) && m.IsGenericMethodDefinition &&
                            m.GetGenericArguments().Length == 2);

            var gmethod = method.MakeGenericMethod(entityType, keyType);
            await (Task)gmethod.Invoke(this, [repoObj, batchSize, deleteMissing, ct])!;
        }
    }

    private static Type? ResolveKeyType(Type entityType)
    {
        var ientity = entityType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
        return ientity?.GetGenericArguments().FirstOrDefault();
    }

    private EntitySearchDescriptor GetDescriptor(Type type)
    {
        if (!options.Value.Descriptors.TryGetValue(type, out var descriptor))
        {
            throw new BusinessException("Lucene:EntityNotConfigured").WithData("Entity", type.FullName);
        }

        return descriptor;
    }

    /// <summary>
    /// 列出索引中所有文档的主键 Id（字符串）。
    /// </summary>
    private async Task<List<string>> ListIndexedIdsAsync(EntitySearchDescriptor descriptor)
    {
        await Task.Yield();
        var indexPath = indexManager.GetIndexPath(descriptor.IndexName);
        using var dir = options.Value.DirectoryFactory(indexPath);
        using var reader = DirectoryReader.Open(dir);

        var ids = new List<string>(reader.MaxDoc);
        for (var docId = 0; docId < reader.MaxDoc; docId++)
        {
            var doc = reader.Document(docId);
            var id = doc.Get(descriptor.IdFieldName);
            if (!string.IsNullOrEmpty(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    private static string InferNameLocal(LambdaExpression exp)
    {
        if (exp.Body is MemberExpression m)
        {
            return m.Member.Name;
        }

        if (exp.Body is UnaryExpression u &&
            u.Operand is MemberExpression um)
        {
            return um.Member.Name;
        }

        return "Field";
    }

    private class IndexShape
    {
        public object? Id { get; set; }
        public object? V0 { get; set; }
        public object? V1 { get; set; }
        public object? V2 { get; set; }
        public object? V3 { get; set; }
        public object? V4 { get; set; }
        public object? V5 { get; set; }
        public object? V6 { get; set; }
        public object? V7 { get; set; }
        public object? V8 { get; set; }
        public object? V9 { get; set; }
        public object? V10 { get; set; }
        public object? V11 { get; set; }
        public object? V12 { get; set; }
        public object? V13 { get; set; }
        public object? V14 { get; set; }
        public object? V15 { get; set; }
    }
}