using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public class TagAppService(ITagRepository repository, TagManager tagManager)
    : CrudAppService<Tag, TagDto, Guid, TagGetListInput, TagCreateUpdateDto, TagCreateUpdateDto>(repository),
        ITagAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Tag.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Tag.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Tag.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Tag.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Tag.Delete;

    public override async Task<TagDto> CreateAsync(TagCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();
        var tag = await tagManager.CreateTagAsync(input.Name);
        await Repository.InsertAsync(tag);
        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    public override async Task<TagDto> UpdateAsync(Guid id, TagCreateUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        var tag = await Repository.GetAsync(id);
        tag.SetName(input.Name);
        await tagManager.UpdateTagAsync(tag);
        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    protected override async Task<IQueryable<Tag>> CreateFilteredQueryAsync(TagGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }
}