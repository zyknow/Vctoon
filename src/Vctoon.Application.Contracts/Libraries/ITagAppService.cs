using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface ITagAppService :
    ICrudAppService<
        TagDto,
        Guid,
        TagGetListInput,
        TagCreateUpdateDto,
        TagCreateUpdateDto>
{
    Task<List<TagDto>> GetAllTagAsync(bool withResourceCount = false);
    Task DeleteManyAsync(List<Guid> ids);
}