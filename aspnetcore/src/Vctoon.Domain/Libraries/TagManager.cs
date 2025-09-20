using Volo.Abp.Domain.Services;

namespace Vctoon.Libraries;

public class TagManager(ITagRepository tagRepository) : DomainService
{
    public async Task<Tag> CreateTagAsync(
        string name
    )
    {
        var tag = new Tag(
            GuidGenerator.Create(),
            name
        );

        await CheckTagNameIsExistAsync(name);
        return await tagRepository.InsertAsync(tag);
    }

    public async Task UpdateTagAsync(
        Tag tag
    )
    {
        await CheckTagNameIsExistAsync(tag.Name, tag.Id);
    }

    private async Task CheckTagNameIsExistAsync(
        string name,
        Guid? excludeId = null
    )
    {
        var query = await tagRepository.GetQueryableAsync();
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