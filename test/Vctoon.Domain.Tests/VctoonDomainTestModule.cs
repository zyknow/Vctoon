using Volo.Abp.Modularity;

namespace Vctoon;

[DependsOn(
    typeof(VctoonDomainModule),
    typeof(VctoonTestBaseModule)
)]
public class VctoonDomainTestModule : AbpModule
{

}
