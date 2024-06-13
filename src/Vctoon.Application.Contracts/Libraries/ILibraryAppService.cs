using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface ILibraryAppService : IApplicationService
{
    Task ScanAsync(Guid libraryId);
    Task<List<LibraryDto>> GetCurrentUserLibraryListAsync();

    Task<LibraryDto> CreateAsync(LibraryCreateUpdateDto input);

    Task<LibraryDto> UpdateAsync(Guid id, LibraryCreateUpdateDto input);

    Task<LibraryDto> GetAsync(Guid id);
    Task DeleteAsync(Guid id);
}