using EasyAbp.Abp.SettingUi;
using Localization.Resources.AbpUi;
using Vctoon.Localization.Vctoon;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;

namespace Vctoon;

[DependsOn(
    typeof(VctoonApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpSettingUiHttpApiModule)
)]
public class VctoonHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureLocalization();
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<VctoonResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }
}