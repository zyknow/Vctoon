using Vctoon.Libraries;
using Vctoon.Localization.Vctoon;
using Vctoon.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Vctoon;

public abstract class VctoonCrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    : CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput> where TEntity : class, IEntity<TKey>
{
    protected VctoonCrudAppService(IRepository<TEntity, TKey> repository, ILibraryPermissionChecker permissionChecker) :
        base(repository)
    {
        LocalizationResource = typeof(VctoonResource);
    }

    protected ILibraryPermissionChecker LibraryPermissionChecker { get; }

    protected Task CheckCurrentUserLibraryPermissionAsync(Guid libraryId, Func<LibraryPermissionCacheItem, bool> checkFunc)
    {
        return LibraryPermissionChecker.CheckAsync(CurrentUser.Id.Value, libraryId, checkFunc);
    }
}