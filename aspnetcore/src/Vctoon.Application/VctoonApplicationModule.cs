using EasyAbp.Abp.SettingUi;
using Microsoft.Extensions.DependencyInjection;
using Vctoon.Handlers;
using Vctoon.ImageProviders;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;

namespace Vctoon;

[DependsOn(
    typeof(VctoonDomainModule),
    typeof(VctoonApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingUiApplicationModule)
)]
public class VctoonApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<VctoonApplicationModule>(); });

        context.Services.AddTransient<IMediumScanHandler,ComicScanHandler>();
        context.Services.AddTransient<IMediumScanHandler,VideoScanHandler>();
        context.Services.AddTransient<IImageProvider,ImageRealFileProvider>();
        

    }
}