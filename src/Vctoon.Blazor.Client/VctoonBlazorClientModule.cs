using Blazor.Store;
using Volo.Abp.Modularity;

namespace Vctoon.Blazor.Client;

[DependsOn(typeof(BlazorStoreModule))]
public class VctoonBlazorClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}