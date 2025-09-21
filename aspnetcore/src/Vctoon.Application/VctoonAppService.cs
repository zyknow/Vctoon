using Vctoon.Localization.Vctoon;

namespace Vctoon;

/* Inherit your application services from this class.
 */
public abstract class VctoonAppService : ApplicationService
{
    protected VctoonAppService()
    {
        LocalizationResource = typeof(VctoonResource);
    }

    protected LibraryPermissionChecker LibraryPermissionChecker =>
        LazyServiceProvider.LazyGetRequiredService<LibraryPermissionChecker>();

    /// <summary>
    /// </summary>
    /// <param name="libraryId"></param>
    /// <param name="checkFunc"></param>
    /// <exception cref="AbpAuthorizationException"></exception>
    /// <returns></returns>
    protected Task CheckCurrentUserLibraryPermissionAsync(Guid libraryId,
        Func<LibraryPermission, bool> checkFunc)
    {
        return LibraryPermissionChecker.CheckAsync(CurrentUser.Id.Value, libraryId, checkFunc);
    }
}