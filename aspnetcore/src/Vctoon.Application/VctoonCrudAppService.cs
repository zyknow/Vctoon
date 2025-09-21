using Vctoon.Localization.Vctoon;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Vctoon;

public abstract class
    VctoonCrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(
        IRepository<TEntity, TKey> repository)
    : VctoonCrudAppService<TEntity, TEntityDto, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>(repository)
    where TEntity : class, IEntity<TKey>;

public abstract class VctoonCrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput,
    TUpdateInput>
    : CrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput,
        TUpdateInput>
    where TEntity : class, IEntity<TKey>
{
    protected VctoonCrudAppService(IRepository<TEntity, TKey> repository) : base(repository)
    {
        LocalizationResource = typeof(VctoonResource);
    }

    protected LibraryPermissionChecker LibraryPermissionChecker =>
        LazyServiceProvider.LazyGetRequiredService<LibraryPermissionChecker>();

    protected Task CheckCurrentUserLibraryPermissionAsync(Guid libraryId,
        Func<LibraryPermission, bool> checkFunc)
    {
        return LibraryPermissionChecker.CheckAsync(CurrentUser.Id.Value, libraryId, checkFunc);
    }
}