using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Vctoon.Libraries;

public class LibraryPermissionStore(
    IDistributedCache<LibraryPermissionCacheItem> cache,
    ILibraryPermissionRepository libraryPermissionRepository,
    ILibraryRepository libraryRepository,
    IIdentityUserRepository identityUserRepository,
    IdentityUserStore identityUserStore)
    : ILibraryPermissionStore, ITransientDependency
{
    public async Task<LibraryPermissionCacheItem> GetOrNullAsync(Guid userId, Guid libraryId)
    {
        return await cache.GetOrAddAsync(LibraryPermissionCacheItem.CalculateCacheKey(userId, libraryId), async () =>
        {
            List<string>? userRoleNames = await identityUserRepository.GetRoleNamesAsync(userId);

            if (userRoleNames.Contains(VctoonSharedConsts.AdminUserRoleName))
            {
                return new LibraryPermissionCacheItem(
                    libraryId,
                    userId,
                    true,
                    true,
                    true,
                    true,
                    true
                );
            }

            LibraryPermission? permission =
                await libraryPermissionRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.LibraryId == libraryId);


            return permission == null
                ? null
                : new LibraryPermissionCacheItem(
                    permission.LibraryId,
                    permission.UserId,
                    permission.CanDownload,
                    permission.CanComment,
                    permission.CanStar,
                    permission.CanView,
                    permission.CanShare
                );
        });
    }

    public async Task RemoveUserPermissionAsync(Guid userId)
    {
        List<Guid> libraryIds =
            await libraryRepository.AsyncExecuter.ToListAsync((await libraryRepository.GetQueryableAsync()).Select(x => x.Id));

        string[] keys = libraryIds.Select(x => LibraryPermissionCacheItem.CalculateCacheKey(userId, x)).ToArray();

        await libraryPermissionRepository.DeleteDirectAsync(x => x.UserId == userId);

        await cache.RemoveManyAsync(keys);
    }

    [UnitOfWork]
    public async Task SetAsync(LibraryPermission permission)
    {
        LibraryPermission? existingPermission =
            await libraryPermissionRepository.FirstOrDefaultAsync(x =>
                x.UserId == permission.UserId && x.LibraryId == permission.LibraryId);

        if (existingPermission == null)
        {
            await libraryPermissionRepository.InsertAsync(permission);
        }
        else
        {
            existingPermission.CanDownload = permission.CanDownload;
            existingPermission.CanComment = permission.CanComment;
            existingPermission.CanStar = permission.CanStar;
            existingPermission.CanView = permission.CanView;
            existingPermission.CanShare = permission.CanShare;

            await libraryPermissionRepository.UpdateAsync(existingPermission);
        }

        await cache.SetAsync(LibraryPermissionCacheItem.CalculateCacheKey(permission.UserId, permission.LibraryId),
            new LibraryPermissionCacheItem(
                permission.LibraryId,
                permission.UserId,
                permission.CanDownload,
                permission.CanComment,
                permission.CanStar,
                permission.CanView,
                permission.CanShare
            ));
    }
}