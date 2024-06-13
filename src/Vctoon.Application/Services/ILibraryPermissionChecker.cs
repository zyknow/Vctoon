using Vctoon.Libraries;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Services;

public interface ILibraryPermissionChecker : ITransientDependency
{
    /// <summary>
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="libraryId"></param>
    /// <param name="checkFunc"></param>
    /// <exception cref="AbpAuthorizationException"></exception>
    Task CheckAsync(Guid? userId, Guid libraryId,
        Func<LibraryPermissionCacheItem, bool> checkFunc);

    Task CheckUserIsAdminAsync(Guid? userId);
}