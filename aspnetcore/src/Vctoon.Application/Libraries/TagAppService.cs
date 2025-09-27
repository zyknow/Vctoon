using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Vctoon.Hubs;
using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;

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
    

    [Route("/api/app/tag/all")]
    public async Task<List<TagDto>> GetAllTagAsync(bool withResourceCount = false)
    {
        await CheckGetListPolicyAsync();

        var tags = await AsyncExecuter.ToListAsync((await Repository.GetQueryableAsync()).Select(x =>
            new TagDto
            {
                Name = x.Name,
                Id = x.Id
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
                typeof(Tag).Name.ToLowerInvariant() , ids);
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