using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Vctoon.Identities;

public class IdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    string[] RoleNames = [VctoonSharedConsts.AdminUserRoleName, VctoonSharedConsts.NormalUserRoleName, VctoonSharedConsts.GhostUserRoleName];
    public IdentityDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            foreach (var roleName in RoleNames)
            {
                var normalizedName = roleName.ToUpperInvariant();
                var role = await _roleRepository.FindByNormalizedNameAsync(normalizedName);
                if (role is null)
                {
                    role = new IdentityRole(_guidGenerator.Create(), roleName, context?.TenantId);

                    if(roleName == VctoonSharedConsts.NormalUserRoleName)
                    {
                        role.IsDefault = true;
                    }

                    role.IsStatic = true;

                    var result = await _roleManager.CreateAsync(role);
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