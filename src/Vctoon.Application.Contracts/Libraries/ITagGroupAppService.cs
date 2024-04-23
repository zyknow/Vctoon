using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface ITagGroupAppService :
    ICrudAppService<
        TagGroupDto,
        Guid,
        TagGroupGetListInput,
        TagGroupCreateUpdateDto,
        TagGroupCreateUpdateDto>
{
    // Task<TagGroupDto> UpdateTagsAsync(Guid tagGroupId, IEnumerable<Guid> tagIds);
}