using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;

namespace Zyknow.Abp.Lucene;

[DependsOn(
    typeof(ZyknowLuceneApplicationModule),
    typeof(Volo.Abp.AspNetCore.AbpAspNetCoreModule)
)]
public class ZyknowLuceneHttpApiModule : AbpModule
{
}
