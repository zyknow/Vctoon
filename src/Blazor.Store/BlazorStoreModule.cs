using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Blazor.Store;

public class BlazorStoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        IServiceCollection services = context.Services;
        services.AddBlazoredSessionStorage();
        services.AddBlazoredLocalStorage();
    }
}