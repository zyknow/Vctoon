using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Vctoon;

public abstract class VctoonAbstractKeyReadOnlyAppService<TEntity, TGetOutputDto, TKey, TGetListInput>(
    IReadOnlyRepository<TEntity> repository,
    LibraryPermissionChecker permissionChecker)
    :
        AbstractKeyReadOnlyAppService<TEntity, TGetOutputDto, TGetOutputDto, TKey, TGetListInput>(repository)
    where TEntity : class, IEntity
{
    protected LibraryPermissionChecker LibraryPermissionChecker { get; } = permissionChecker;

    protected Task CheckCurrentUserLibraryPermissionAsync(Guid libraryId,
        Func<LibraryPermission, bool> checkFunc)
    {
        return LibraryPermissionChecker.CheckAsync(CurrentUser.Id.Value, libraryId, checkFunc);
    }
}