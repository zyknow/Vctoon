using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace Vctoon.Permissions;

public class IdentityUserExtraPermissionDataSender(
    IPermissionGrantRepository permissionGrantRepository,
    IRepository<IdentityRole> identityRoleRepository,
    IdentityRoleStore identityRoleStore,
    IGuidGenerator guidGenerator,
    ILogger<IdentityUserExtraPermissionDataSender> logger)
    : ITransientDependency
{
    protected readonly List<string> GhostUserRolePermissions =
    [
        IdentityPermissions.Users.Default,
        VctoonPermissions.Tag.Default,
        VctoonPermissions.Comic.Default,
        VctoonPermissions.Library.Default
    ];

    protected readonly List<string> NormalUserRolePermissions =
    [
        IdentityPermissions.Users.Default,
        VctoonPermissions.Tag.Default,
        VctoonPermissions.Comic.Default,
        VctoonPermissions.Library.Default
    ];

    [UnitOfWork]
    public async Task SendAsync()
    {
        logger.LogInformation("Start to send extra permissions to identity roles");
        List<IdentityRole> roles = await identityRoleRepository.GetListAsync();

        // normalUserRole
        {
            IdentityRole? normalUserRole = roles.FirstOrDefault(x => x.Name == VctoonSharedConsts.NormalUserRoleName);
            if (normalUserRole == null)
            {
                normalUserRole = new IdentityRole(guidGenerator.Create(), VctoonSharedConsts.NormalUserRoleName);
                await identityRoleRepository.InsertAsync(normalUserRole);
            }

            await SetRolePermissionAsync(normalUserRole.Name, NormalUserRolePermissions);
        }

        // ghostUserRole
        {
            IdentityRole? ghostUserRole = roles.FirstOrDefault(x => x.Name == VctoonSharedConsts.GhostUserRoleName);
            if (ghostUserRole == null)
            {
                ghostUserRole = new IdentityRole(guidGenerator.Create(), VctoonSharedConsts.GhostUserRoleName);
                await identityRoleRepository.InsertAsync(ghostUserRole);
            }

            await SetRolePermissionAsync(ghostUserRole.Name, GhostUserRolePermissions);
        }
    }

    private async Task SetRolePermissionAsync(string roleName, List<string> permissions)
    {
        List<string>? existsPermissionGrants =
            (await permissionGrantRepository.GetListAsync(RolePermissionValueProvider.ProviderName, roleName))
            .Select(x => x.Name)
            .ToList();

        List<PermissionGrant> addPermissions = permissions.Except(existsPermissionGrants).Select(permissionName =>
            new PermissionGrant(
                guidGenerator.Create(),
                permissionName,
                RolePermissionValueProvider.ProviderName,
                roleName
            )).ToList();

        if (!addPermissions.IsNullOrEmpty())
        {
            await permissionGrantRepository.InsertManyAsync(addPermissions, true);
        }
    }
}