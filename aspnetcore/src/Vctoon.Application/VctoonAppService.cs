using Microsoft.AspNetCore.SignalR;
using Vctoon.Hubs;
using Vctoon.Localization.Vctoon;
using Volo.Abp.Authorization;

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

    protected IHubContext<DataChangedHub> DataChangedHub => LazyServiceProvider.LazyGetRequiredService<IHubContext<DataChangedHub>>();

    
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