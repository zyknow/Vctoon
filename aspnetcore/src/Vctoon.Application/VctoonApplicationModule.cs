using EasyAbp.Abp.SettingUi;
using Microsoft.Extensions.DependencyInjection;
using Vctoon.Handlers;
using Vctoon.ImageProviders;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Zyknow.Abp.Lucene;

namespace Vctoon;

[DependsOn(
    typeof(VctoonDomainModule),
    typeof(VctoonApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingUiApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(ZyknowLuceneApplicationModule)
)]
public class VctoonApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<VctoonApplicationModule>();

        context.Services.AddTransient<IMediumScanHandler, ComicScanHandler>();
        context.Services.AddTransient<IMediumScanHandler, VideoScanHandler>();
        context.Services.AddTransient<IImageProvider, ImageRealFileProvider>();
    }
}