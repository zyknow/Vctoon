using Microsoft.FluentUI.AspNetCore.Components;
using Vctoon.Blazor.Client.Validations;
using Volo.Abp.AutoMapper;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.UI;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client;

[DependsOn(typeof(VctoonHttpApiClientModule), typeof(BlazorStoreModule), typeof(AbpUiModule), typeof(AbpAutoMapperModule))]
public class VctoonBlazorClientModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpHttpClientBuilderOptions>(options =>
        {
            options.ProxyClientBuildActions.Add((s, builder) =>
            {
                builder.AddHttpMessageHandler<AuthorizationMessageHandler>();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<VctoonBlazorClientModule>(); });

        services.AddFluentUIComponents();

        Configure<AbpValidationOptions>(opt =>
        {
            opt.ObjectValidationContributors.Clear();
            opt.ObjectValidationContributors.Add<BlazorDataAnnotationObjectValidationContributor>();
        });
    }
}