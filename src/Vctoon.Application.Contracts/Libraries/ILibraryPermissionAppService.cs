using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface ILibraryPermissionAppService :
    ICrudAppService<
        LibraryPermissionDto,
        Guid,
        PagedAndSortedResultRequestDto,
        LibraryPermissionCreateUpdateDto,
        LibraryPermissionCreateUpdateDto>
{
}