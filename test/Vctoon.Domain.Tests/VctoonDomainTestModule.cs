using Vctoon.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Vctoon;

[DependsOn(
    typeof(VctoonEntityFrameworkCoreTestModule)
)]
public class VctoonDomainTestModule : AbpModule
{

}
