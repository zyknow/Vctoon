using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries;

public class LibraryPermissionAppService : CrudAppService<LibraryPermission, LibraryPermissionDto, Guid,
        PagedAndSortedResultRequestDto, LibraryPermissionCreateUpdateDto, LibraryPermissionCreateUpdateDto>,
    ILibraryPermissionAppService
{
    private readonly ILibraryPermissionRepository _repository;
    
    public LibraryPermissionAppService(ILibraryPermissionRepository repository) : base(repository)
    {
        _repository = repository;
    }
}