using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;

namespace Zyknow.Abp.Lucene;

[DependsOn(
    typeof(ZyknowLuceneApplicationModule),
    typeof(AbpAspNetCoreModule)
)]
public class ZyknowLuceneHttpApiModule : AbpModule
{
}