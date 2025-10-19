using Volo.Abp.Modularity;

namespace Zyknow.Abp.Lucene;

[DependsOn(
    typeof(ZyknowLuceneDomainSharedModule)
)]
public class ZyknowLuceneDomainModule : AbpModule
{
}