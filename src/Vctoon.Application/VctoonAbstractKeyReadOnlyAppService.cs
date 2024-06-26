﻿using Vctoon.Libraries;
using Vctoon.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Vctoon;

public abstract class VctoonAbstractKeyReadOnlyAppService<TEntity, TGetOutputDto, TKey, TGetListInput>(
    IReadOnlyRepository<TEntity> repository,
    ILibraryPermissionChecker permissionChecker)
    :
        AbstractKeyReadOnlyAppService<TEntity, TGetOutputDto, TGetOutputDto, TKey, TGetListInput>(repository)
    where TEntity : class, IEntity
{
    protected ILibraryPermissionChecker LibraryPermissionChecker { get; } = permissionChecker;

    protected Task CheckCurrentUserLibraryPermissionAsync(Guid libraryId, Func<LibraryPermissionCacheItem, bool> checkFunc)
    {
        return LibraryPermissionChecker.CheckAsync(CurrentUser.Id.Value, libraryId, checkFunc);
    }
}