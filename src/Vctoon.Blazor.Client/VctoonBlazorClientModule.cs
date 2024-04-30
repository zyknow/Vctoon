using Blazor.Store;
using Microsoft.FluentUI.AspNetCore.Components;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client;

[DependsOn(typeof(BlazorStoreModule), typeof(VctoonApplicationContractsModule), typeof(AbpValidationModule))]
public class VctoonBlazorClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddHttpClient();
        services.AddFluentUIComponents();
    }
}