using Localization.Resources.AbpUi;
using Vctoon.Localization.Comics;
using Vctoon.Localization.Libraries;
using Vctoon.Localization.Vctoon;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.UI;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Vctoon;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule),
    typeof(AbpUiModule)
)]
public class VctoonDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        VctoonGlobalFeatureConfigurator.Configure();
        VctoonModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options => { options.FileSets.AddEmbedded<VctoonDomainSharedModule>(); });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<LibraryResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddBaseTypes(typeof(AbpUiResource))
                .AddVirtualJson("/Localization/Libraries");

            options.Resources
                .Add<ComicResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddBaseTypes(typeof(AbpUiResource))
                .AddVirtualJson("/Localization/Comics");

            options.Resources
                .Get<IdentityResource>()
                .AddVirtualJson("/Localization/Identities");

            options.Resources
                .Get<AbpUiResource>()
                .AddVirtualJson("/Localization/Ui");

            options.Resources
                .Add<VctoonResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddBaseTypes(typeof(LibraryResource))
                .AddBaseTypes(typeof(ComicResource))
                .AddVirtualJson("/Localization/Vctoon");

            options.DefaultResourceType = typeof(VctoonResource);
        });

        Configure<AbpExceptionLocalizationOptions>(options => { options.MapCodeNamespace($"Vctoon", typeof(VctoonResource)); });
    }
}