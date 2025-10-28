namespace Vctoon.Libraries;

public interface ILibraryAppService : IApplicationService
{
    Task<LibraryDto> CreateAsync(LibraryCreateUpdateDto input);

    Task<LibraryDto> UpdateAsync(Guid id, LibraryCreateUpdateDto input);

    Task<LibraryDto> GetAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task<List<LibraryDto>> GetCurrentUserLibraryListAsync();
    Task ScanAsync(Guid libraryId);
    Task UpdateLibraryPermissionsAsync(Guid userId, Guid libraryId, LibraryPermissionCreateUpdateDto input);
    Task<LibraryPermissionDto> GetLibraryPermissionsAsync(Guid userId, Guid libraryId);

    // Users that can be assigned library permissions (not in Normal role)
    Task<IPagedResult<IdentityUserDto>> GetAssignableUsersAsync(GetIdentityUsersInput input);
}