using EasyAbp.Abp.SettingUi;
using Volo.Abp.Account;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Zyknow.Abp.Lucene;

namespace Vctoon;

[DependsOn(
    typeof(VctoonDomainSharedModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpSettingUiApplicationContractsModule),
    typeof(ZyknowLuceneApplicationContractsModule)
)]
public class VctoonApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        VctoonDtoExtensions.Configure();
    }
}