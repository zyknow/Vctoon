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
}