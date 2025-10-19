using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus; // changed from .Local to correct namespace
using Volo.Abp.Uow;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Services;

namespace Zyknow.Abp.Lucene.Indexing.Handlers;

/// <summary>
/// 通用的实体事件处理器：在同一 UnitOfWork 中聚合创建/更新/删除事件，
/// 并在事务提交后一次性批量写入 Lucene 索引。
/// 仅对已通过 Fluent DSL 注册的实体类型生效。
/// </summary>
public sealed class GenericIndexingHandler<T> :
    ILocalEventHandler<EntityCreatedEventData<T>>,
    ILocalEventHandler<EntityUpdatedEventData<T>>,
    ILocalEventHandler<EntityDeletedEventData<T>>,
    ITransientDependency
{
    private readonly IIndexingCollector _collector;
    private readonly IUnitOfWorkManager _uow;
    private readonly LuceneIndexManager _indexer;
    private readonly IOptions<LuceneOptions> _options;

    public GenericIndexingHandler(
        IIndexingCollector collector,
        IUnitOfWorkManager uow,
        LuceneIndexManager indexer,
        IOptions<LuceneOptions> options)
    {
        _collector = collector;
        _uow = uow;
        _indexer = indexer;
        _options = options;
    }

    public Task HandleEventAsync(EntityCreatedEventData<T> eventData) => HandleUpsert(eventData.Entity);

    public Task HandleEventAsync(EntityUpdatedEventData<T> eventData) => HandleUpsert(eventData.Entity);

    public Task HandleEventAsync(EntityDeletedEventData<T> eventData)
    {
        if (!IsConfigured()) return Task.CompletedTask;
        var id = GetIdString(eventData.Entity);
        if (id != null)
        {
            _collector.Delete<T>(id);
            RegisterOnce();
        }
        return Task.CompletedTask;
    }

    private Task HandleUpsert(T entity)
    {
        if (!IsConfigured()) return Task.CompletedTask;
        var id = GetIdString(entity);
        if (id != null)
        {
            _collector.Upsert(entity, id);
            RegisterOnce();
        }
        return Task.CompletedTask;
    }

    private bool IsConfigured()
    {
        var opts = _options.Value;
        if (!opts.EnableAutoIndexingEvents) return false;
        return opts.Descriptors.ContainsKey(typeof(T));
    }

    private void RegisterOnce()
    {
        var current = _uow.Current;
        if (current != null)
        {
            _collector.RegisterOnCompleted(current, _indexer);
        }
        else
        {
            // 无 UoW 情况：立即执行批处理
            _collector.ProcessImmediatelyAsync(_indexer).GetAwaiter().GetResult();
        }
    }

    private static string? GetIdString(T entity)
    {
        if (entity == null) return null;
        // 优先使用 Guid 主键的接口（项目约定使用 Guid）
        if (entity is Volo.Abp.Domain.Entities.IEntity<Guid> g) return g.Id.ToString();
        // 反射回退
        var prop = entity.GetType().GetProperty("Id");
        var val = prop?.GetValue(entity);
        return val?.ToString();
    }

}