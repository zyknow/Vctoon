using Volo.Abp.Modularity;

namespace Vctoon.Blazor.Client;

public class VctoonBlazorClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        IServiceCollection services = context.Services;
    }
}