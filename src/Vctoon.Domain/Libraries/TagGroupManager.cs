using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Vctoon.Libraries;

public class TagGroupManager(ITagGroupRepository tagGroupRepository) : DomainService
{
    public async Task<TagGroup> CreateTagGroupAsync(
        string name
    )
    {
        var tagGroup = new TagGroup(
            GuidGenerator.Create(),
            name
        );

        await CheckTagGroupNameIsExistAsync(name);
        tagGroup = await tagGroupRepository.InsertAsync(tagGroup);
        return tagGroup;
    }

    public async Task UpdateTagGroupAsync(
        TagGroup tagGroup
    )
    {
        await CheckTagGroupNameIsExistAsync(tagGroup.Name, tagGroup.Id);
    }

    public async Task UpdateTagListAsync(TagGroup tagGroup, IEnumerable<Guid> tagIds)
    {
        await tagGroupRepository.EnsureCollectionLoadedAsync(tagGroup, x => x.Tags);

        var removeTags = tagGroup.Tags
            .Where(x => !tagIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();
        tagGroup.RemoveTags(removeTags);

        var addTags = tagIds
            .Where(x => !tagGroup.Tags.Select(x => x.Id).Contains(x))
            // the shadow table is only need set id, name is not important,the performance better than query tags from database
            .Select(id => new Tag(id, id.ToString()))
            .ToList();

        tagGroup.AddTags(addTags);
    }


    private async Task CheckTagGroupNameIsExistAsync(
        string name,
        Guid? excludeId = null
    )
    {
        var query = await tagGroupRepository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(
                query
                    .WhereIf(excludeId.HasValue, x => x.Id != excludeId)
                    .Where(x => x.Name == name)
            ))
        {
            throw new BusinessException(VctoonDomainErrorCodes.NameIsAlreadyExists).WithData("Name", name);
        }
    }
}