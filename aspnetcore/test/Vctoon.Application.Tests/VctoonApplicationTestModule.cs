using Volo.Abp.Modularity;

namespace Vctoon;

[DependsOn(
    typeof(VctoonApplicationModule),
    typeof(VctoonDomainTestModule)
)]
public class VctoonApplicationTestModule : AbpModule
{

}
