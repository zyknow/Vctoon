using EasyAbp.Abp.SettingUi;
using EasyAbp.Abp.SettingUi.Localization;
using Vctoon.Localization.Vctoon;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using Zyknow.Abp.Lucene;

namespace Vctoon;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(BlobStoringDatabaseDomainSharedModule),
    typeof(AbpSettingUiDomainSharedModule),
    typeof(ZyknowLuceneDomainSharedModule)
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
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<VctoonDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<IdentityResource>()
                .AddVirtualJson("/Localization/Identities");

            options.Resources
                .Add<VctoonResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Vctoon");
            
            options.Resources
                .Get<SettingUiResource>()
                .AddVirtualJson("/Localization/SettingUI");

            options.DefaultResourceType = typeof(VctoonResource);

            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "Chinese (Simplified)"));
            // options.Languages.Add(new LanguageInfo("en-GB", "en-GB", "English (United Kingdom)"));
            // options.Languages.Add(new LanguageInfo("es", "es", "Spanish"));
            // options.Languages.Add(new LanguageInfo("ar", "ar", "Arabic"));
            // options.Languages.Add(new LanguageInfo("hi", "hi", "Hindi "));
            // options.Languages.Add(new LanguageInfo("pt-BR", "pt-BR", "Portuguese (Brazil)"));
            // options.Languages.Add(new LanguageInfo("fr", "fr", "French"));
            // options.Languages.Add(new LanguageInfo("ru", "ru", "Russian"));
            // options.Languages.Add(new LanguageInfo("de-DE", "de-DE", "German (Germany)"));
            // options.Languages.Add(new LanguageInfo("tr", "tr", "Turkish"));
            // options.Languages.Add(new LanguageInfo("it", "it", "Italian"));
            // options.Languages.Add(new LanguageInfo("cs", "cs", "Czech"));
            // options.Languages.Add(new LanguageInfo("hu", "hu", "Hungarian"));
            // options.Languages.Add(new LanguageInfo("ro-RO", "ro-RO", "Romanian (Romania)"));
            // options.Languages.Add(new LanguageInfo("sv", "sv", "Swedish"));
            // options.Languages.Add(new LanguageInfo("fi", "fi", "Finnish"));
            // options.Languages.Add(new LanguageInfo("sk", "sk", "Slovak"));
            // options.Languages.Add(new LanguageInfo("is", "is", "Icelandic"));
            // options.Languages.Add(new LanguageInfo("zh-Hant", "zh-Hant", "Chinese (Traditional)"));
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Vctoon", typeof(VctoonResource));
        });
    }
}