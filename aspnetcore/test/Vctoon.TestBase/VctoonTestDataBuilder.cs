using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Vctoon;

public class VctoonTestDataSeedContributor(ICurrentTenant currentTenant) : IDataSeedContributor, ITransientDependency
{
    public Task SeedAsync(DataSeedContext context)
    {
        /* Seed additional test data... */

        using (currentTenant.Change(context?.TenantId))
        {
            return Task.CompletedTask;
        }
    }
}