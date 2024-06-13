namespace Vctoon.Libraries;

public interface ILibraryPermissionStore
{
    Task<LibraryPermissionCacheItem> GetOrNullAsync(Guid userId, Guid libraryId);
    Task SetAsync(LibraryPermission permission);
    Task RemoveUserPermissionAsync(Guid userId);
}