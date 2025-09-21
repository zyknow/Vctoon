using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Vctoon.Identities;

public class IdentityDataSeedContributor(
    IIdentityRoleRepository roleRepository,
    IdentityRoleManager roleManager,
    ICurrentTenant currentTenant,
    IGuidGenerator guidGenerator)
    : IDataSeedContributor, ITransientDependency
{
    private string[] RoleNames =
    [
        VctoonSharedConsts.AdminUserRoleName, VctoonSharedConsts.NormalUserRoleName,
        VctoonSharedConsts.GhostUserRoleName
    ];

    public async Task SeedAsync(DataSeedContext context)
    {
        using (currentTenant.Change(context?.TenantId))
        {
            foreach (var roleName in RoleNames)
            {
                var normalizedName = roleName.ToUpperInvariant();
                var role = await roleRepository.FindByNormalizedNameAsync(normalizedName);
                if (role is null)
                {
                    role = new IdentityRole(guidGenerator.Create(), roleName, context?.TenantId);

                    if (roleName == VctoonSharedConsts.NormalUserRoleName)
                    {
                        role.IsDefault = true;
                    }

                    role.IsStatic = true;

                    var result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        var errorMessage = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new AbpException($"Failed to create role '{roleName}': {errorMessage}");
                    }
                }
            }
        }

        return; // explicit return for async Task
    }
}