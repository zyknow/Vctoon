using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Modularity;
using Zyknow.Abp.Lucene.Indexing.Handlers;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Services;

namespace Zyknow.Abp.Lucene;

[DependsOn(
    typeof(ZyknowLuceneDomainModule),
    typeof(ZyknowLuceneDomainSharedModule)
)]
public class ZyknowLuceneApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ILuceneService, LuceneAppService>();
        context.Services.AddTransient(typeof(GenericIndexingHandler<>));
        // 注册 LuceneIndexManager（用于事件处理器批量写入索引）
        context.Services.AddTransient<LuceneIndexManager>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var sp = context.ServiceProvider;
        var options = sp.GetRequiredService<IOptions<LuceneOptions>>().Value;
        if (!options.EnableAutoIndexingEvents)
        {
            return;
        }

        var eventBus = sp.GetService<ILocalEventBus>();
        if (eventBus == null)
        {
            return; // 容器未提供本地事件总线时跳过
        }

        foreach (var entityType in options.Descriptors.Keys)
        {
            var handlerType = typeof(GenericIndexingHandler<>).MakeGenericType(entityType);
            var handler = sp.GetRequiredService(handlerType);

            Subscribe(eventBus, typeof(EntityCreatedEventData<>).MakeGenericType(entityType), handler);
            Subscribe(eventBus, typeof(EntityUpdatedEventData<>).MakeGenericType(entityType), handler);
            Subscribe(eventBus, typeof(EntityDeletedEventData<>).MakeGenericType(entityType), handler);
        }
    }

    private static void Subscribe(ILocalEventBus bus, Type eventType, object handler)
    {
        var method = typeof(ILocalEventBus).GetMethods()
            .First(m => m.Name == nameof(ILocalEventBus.Subscribe) && m.IsGenericMethod &&
                        m.GetParameters().Length == 1);
        var gmethod = method.MakeGenericMethod(eventType);
        gmethod.Invoke(bus, [handler]);
    }
}