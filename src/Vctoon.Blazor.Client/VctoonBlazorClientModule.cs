using Microsoft.FluentUI.AspNetCore.Components;
using Vctoon.Blazor.Client.Validations;
using Volo.Abp.Modularity;
using Volo.Abp.UI;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client;

[DependsOn(typeof(VctoonHttpApiClientModule), typeof(BlazorStoreModule), typeof(AbpUiModule))]
public class VctoonBlazorClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        
        // services.AddHttpClient();
        services.AddFluentUIComponents();
        
        Configure<AbpValidationOptions>(opt =>
        {
            opt.ObjectValidationContributors.Clear();
            opt.ObjectValidationContributors.Add<BlazorDataAnnotationObjectValidationContributor>();
        });
    }
}