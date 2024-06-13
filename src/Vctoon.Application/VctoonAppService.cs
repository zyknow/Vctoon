using Vctoon.Libraries;
using Vctoon.Localization.Vctoon;
using Vctoon.Services;
using Volo.Abp.Authorization;

namespace Vctoon;

/* Inherit your application services from this class.
 */
public abstract class VctoonAppService : ApplicationService
{
    protected VctoonAppService(ILibraryPermissionChecker permissionChecker)
    {
        LibraryPermissionChecker = permissionChecker;
        LocalizationResource = typeof(VctoonResource);
    }

    protected ILibraryPermissionChecker LibraryPermissionChecker { get; }

    /// <summary>
    /// </summary>
    /// <param name="libraryId"></param>
    /// <param name="checkFunc"></param>
    /// <exception cref="AbpAuthorizationException"></exception>
    /// <returns></returns>
    protected Task CheckCurrentUserLibraryPermissionAsync(Guid libraryId, Func<LibraryPermissionCacheItem, bool> checkFunc)
    {
        return LibraryPermissionChecker.CheckAsync(CurrentUser.Id.Value, libraryId, checkFunc);
    }
}