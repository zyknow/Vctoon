using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Zyknow.Abp.Lucene;

[DependsOn(
    typeof(AbpDddDomainSharedModule)
)]
public class ZyknowLuceneDomainSharedModule : AbpModule
{
}