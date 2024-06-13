using Volo.Abp.Users;

namespace Vctoon.Extensions;

public static class CurrentUserExtensions
{
    public static bool IsAdmin(this ICurrentUser currentUser)
    {
        return currentUser.IsInRole(VctoonSharedConsts.AdminUserRoleName);
    }
}