using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public class TagGroupAppService(ITagGroupRepository repository, TagGroupManager tagGroupManager)
    : CrudAppService<TagGroup, TagGroupDto, Guid, TagGroupGetListInput,
            TagGroupCreateUpdateDto, TagGroupCreateUpdateDto>(repository),
        ITagGroupAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.TagGroup.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.TagGroup.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.TagGroup.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.TagGroup.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.TagGroup.Delete;

    public override async Task<TagGroupDto> CreateAsync(TagGroupCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();
        var tagGroup = await tagGroupManager.CreateTagGroupAsync(input.Name);
        await Repository.InsertAsync(tagGroup);
        return ObjectMapper.Map<TagGroup, TagGroupDto>(tagGroup);
    }

    public override async Task<TagGroupDto> UpdateAsync(Guid id, TagGroupCreateUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        var tagGroup = await Repository.GetAsync(id);
        tagGroup.SetName(input.Name);
        await tagGroupManager.UpdateTagGroupAsync(tagGroup);
        return ObjectMapper.Map<TagGroup, TagGroupDto>(tagGroup);
    }

    protected override async Task<IQueryable<TagGroup>> CreateFilteredQueryAsync(TagGroupGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }

    public async Task<TagGroupDto> UpdateTagListAsync(Guid tagGroupId, IEnumerable<Guid> tagIds)
    {
        await CheckUpdatePolicyAsync();
        var tagGroup =
            await AsyncExecuter.FirstOrDefaultAsync(
                (await Repository.WithDetailsAsync()).Where(x => x.Id == tagGroupId));

        if (tagGroup == null)
        {
            throw new BusinessException(VctoonDomainErrorCodes.DataNotFound);
        }

        await tagGroupManager.UpdateTagListAsync(tagGroup, tagIds);

        return ObjectMapper.Map<TagGroup, TagGroupDto>(tagGroup);
    }
}