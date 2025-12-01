using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Vctoon.Hubs;
using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

[Authorize]
public class TagAppService(ITagRepository repository)
    : VctoonCrudAppService<Tag, TagDto, Guid, TagGetListInput, TagCreateUpdateDto, TagCreateUpdateDto>(repository),
        ITagAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Tag.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Tag.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Tag.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Tag.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Tag.Delete;

    protected override bool EnabledDataChangedHubNotify => true;


    public override async Task<TagDto> CreateAsync(TagCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();
        var existing = await Repository.FirstOrDefaultAsync(x => x.Name == input.Name);
        if (existing != null)
        {
            throw new UserFriendlyException(L["TagAlreadyExists", input.Name]);
        }

        var entity = new Tag(GuidGenerator.Create(), input.Name);
        await Repository.InsertAsync(entity, true);
        var dto = ObjectMapper.Map<Tag, TagDto>(entity);

        UnitOfWorkManager.Current!.OnCompleted(async () =>
        {
            await DataChangedHub.Clients.All.SendAsync(HubEventConst.DataChangedHub.OnCreated,
                typeof(Tag).Name.ToLowerInvariant(), new List<TagDto> { dto });
        });

        return dto;
    }

    public override async Task<TagDto> UpdateAsync(Guid id, TagCreateUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await GetEntityByIdAsync(id);
        entity.SetName(input.Name);
        await Repository.UpdateAsync(entity, true);
        var dto = ObjectMapper.Map<Tag, TagDto>(entity);

        UnitOfWorkManager.Current!.OnCompleted(async () =>
        {
            await DataChangedHub.Clients.All.SendAsync(HubEventConst.DataChangedHub.OnUpdated,
                typeof(Tag).Name.ToLowerInvariant(), new List<TagDto> { dto });
        });

        return dto;
    }


    [Route("/api/app/tag/all")]
    public async Task<List<TagDto>> GetAllTagAsync(bool withResourceCount = false)
    {
        await CheckGetListPolicyAsync();

        var tags = await AsyncExecuter.ToListAsync((await Repository.GetQueryableAsync()).Select(x =>
            new TagDto
            {
                Name = x.Name,
                Id = x.Id,
            }));
        return tags;
    }

    public async Task DeleteManyAsync(List<Guid> ids)
    {
        await CheckDeletePolicyAsync();
        await Repository.DeleteManyAsync(ids);

        UnitOfWorkManager.Current!.OnCompleted(async () =>
        {
            await DataChangedHub.Clients.All.SendAsync(HubEventConst.DataChangedHub.OnDeleted,
                typeof(Tag).Name.ToLowerInvariant(), ids);
        });
    }

    protected override async Task<IQueryable<Tag>> CreateFilteredQueryAsync(TagGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!))
            ;
    }
}