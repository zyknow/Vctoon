using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Zyknow.Abp.Lucene.Localization;

namespace Zyknow.Abp.Lucene;

[DependsOn(
    typeof(AbpDddDomainSharedModule)
)]
public class ZyknowLuceneDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ZyknowLuceneDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<ZyknowLuceneResource>("en")
                .AddVirtualJson("/Zyknow.Abp.Lucene/Localization/ZyknowLucene");
        });
    }
}