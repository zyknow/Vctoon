using Vctoon.Identities.Dtos;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Vctoon.Identities;

public interface IIdentityUserExtraAppService : IApplicationService
{
    Task<PagedResultDto<IdentityUserExtraDto>> GetUserExtraListAsync(GetIdentityUsersInput input);
    Task<IdentityUserExtraDto> CreateUserExtraAsync(IdentityUserExtraCreateDto input);

    Task<IdentityUserExtraDto> UpdateUserExtraAsync(Guid userId,
        IdentityUserExtraUpdateDto input);

    Task<List<LibraryPermissionDto>> GetPermissionsAsync(Guid userId);
    Task UpdatePermissionsAsync(Guid userId, List<LibraryPermissionCreateUpdateDto> input);
    Task<List<LibraryPermissionDto>> GetCurrentUserLibraryPermissionsAsync();
    Task<IdentityUserExtraDto> GetUserExtraAsync(Guid userId);
    Task DeleteUserExtraAsync(Guid userId);
}