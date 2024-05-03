using Blazor.Store;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using Vctoon.Blazor.Client.Validations;
using Volo.Abp;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client;

[DependsOn(typeof(VctoonApplicationContractsModule),typeof(BlazorStoreModule))]
public class VctoonBlazorClientModule : AbpModule
{



    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddHttpClient();
        services.AddFluentUIComponents();

        Configure<AbpValidationOptions>(opt =>
        {
            opt.ObjectValidationContributors.Clear();
            opt.ObjectValidationContributors.Add<BlazorDataAnnotationObjectValidationContributor>();
        });
    }
}