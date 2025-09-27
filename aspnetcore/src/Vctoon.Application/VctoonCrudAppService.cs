using Microsoft.AspNetCore.SignalR;
using Vctoon.Hubs;
using Vctoon.Localization.Vctoon;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

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

    protected IHubContext<DataChangedHub> DataChangedHub =>
        LazyServiceProvider.LazyGetRequiredService<IHubContext<DataChangedHub>>();

    protected IUnitOfWorkManager UnitOfWorkManager => LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();

    protected virtual bool EnabledDataChangedHubNotify => false;

    protected LibraryPermissionChecker LibraryPermissionChecker =>
        LazyServiceProvider.LazyGetRequiredService<LibraryPermissionChecker>();

    protected Task CheckCurrentUserLibraryPermissionAsync(Guid libraryId,
        Func<LibraryPermission, bool> checkFunc)
    {
        return LibraryPermissionChecker.CheckAsync(CurrentUser.Id.Value, libraryId, checkFunc);
    }

    public override async Task<TGetOutputDto> CreateAsync(TCreateInput input)
    {
        var dto = await base.CreateAsync(input);
        if (EnabledDataChangedHubNotify)
        {
            UnitOfWorkManager.Current!.OnCompleted(async () =>
            {
                await DataChangedHub.Clients.All.SendAsync(HubEventConst.DataChangedHub.OnCreated,
                    typeof(TEntity).Name.ToLowerInvariant(), new List<TGetOutputDto>()
                    {
                        dto
                    });
            });
        }

        return dto;
    }

    public override async Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input)
    {
        var dto = await base.UpdateAsync(id, input);
        if (EnabledDataChangedHubNotify)
        {
            UnitOfWorkManager.Current!.OnCompleted(async () =>
            {
                await DataChangedHub.Clients.All.SendAsync(HubEventConst.DataChangedHub.OnUpdated,
                    typeof(TEntity).Name.ToLowerInvariant() , new List<TGetOutputDto>()
                    {
                        dto
                    });
            });
        }

        return dto;
    }

    public override async Task DeleteAsync(TKey id)
    {
        await base.DeleteAsync(id);
        if (EnabledDataChangedHubNotify)
        {
            UnitOfWorkManager.Current!.OnCompleted(async () =>
            {
                await DataChangedHub.Clients.All.SendAsync(HubEventConst.DataChangedHub.OnDeleted,
                    typeof(TEntity).Name.ToLowerInvariant() , new List<TKey>()
                    {
                        id
                    });
            });
        }
    }
}