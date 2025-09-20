using Vctoon.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Vctoon.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(VctoonEntityFrameworkCoreModule),
    typeof(VctoonApplicationContractsModule)
)]
public class VctoonDbMigratorModule : AbpModule
{
}