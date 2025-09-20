using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Authorization;
using Volo.Abp.Identity;

namespace Vctoon.Services;

public class LibraryPermissionChecker(LibraryPermissionStore libraryPermissionStore) : ITransientDependency
{
    public IAbpLazyServiceProvider LazyServiceProvider { get; set; } = default!;

    /// <summary>
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="libraryId"></param>
    /// <param name="checkFunc"></param>
    /// <exception cref="AbpAuthorizationException"></exception>
    public async Task CheckAsync(Guid? userId, Guid libraryId,
        Func<LibraryPermission, bool> checkFunc)
    {
        if (!userId.HasValue)
        {
            throw new AbpAuthorizationException(@$"can not get user id to check permission for libraryId:{libraryId}");
        }

        var permission = await libraryPermissionStore.GetOrNullAsync(userId.Value, libraryId);
        if (permission == null)
        {
            throw new AbpAuthorizationException(@$"for libraryId:{libraryId} has not permission");
        }

        if (!checkFunc(permission))
        {
            throw new AbpAuthorizationException(@$"for libraryId:{libraryId} has not permission");
        }
    }

    public async Task CheckUserIsAdminAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            throw new AbpAuthorizationException(@"can not get user id to check permission");
        }

        var userStore = LazyServiceProvider.GetRequiredService<IdentityUserStore>();

        var identity = await userStore.FindByIdAsync(userId.Value.ToString());

        if (!await userStore.IsInRoleAsync(identity, VctoonSharedConsts.AdminUserRoleName.ToUpper()))
        {
            throw new AbpAuthorizationException(@$"user:{userId} is not admin");
        }
    }
}