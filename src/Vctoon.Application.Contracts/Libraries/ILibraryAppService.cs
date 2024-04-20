using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface ILibraryAppService :
    ICrudAppService<
        LibraryDto,
        Guid,
        LibraryGetListInput,
        LibraryCreateUpdateDto,
        LibraryCreateUpdateDto>
{
}