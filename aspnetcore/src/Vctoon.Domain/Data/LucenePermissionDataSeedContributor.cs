using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;

namespace Vctoon.Data;

public class LucenePermissionDataSeedContributor(
    IPermissionDataSeeder permissionDataSeeder)
    : IDataSeedContributor, ITransientDependency
{
    public async Task SeedAsync(DataSeedContext context)
    {
        var adminRoleName = VctoonSharedConsts.NormalUserRoleName;

        await permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            adminRoleName,
            new[]
            {
                "ZyknowLucene.Search"
            },
            context?.TenantId
        );
    }
}