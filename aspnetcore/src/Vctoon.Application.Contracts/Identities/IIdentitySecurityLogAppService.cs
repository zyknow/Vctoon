using Vctoon.Identities.Dtos;

namespace Vctoon.Identities;

public interface IIdentitySecurityLogAppService : IApplicationService
{
    Task<PagedResultDto<IdentitySecurityLogDto>> GetListAsync(
        GetIdentitySecurityLogListInput input);

    Task<PagedResultDto<IdentitySecurityLogDto>> GetCurrentUserListAsync(
        GetIdentitySecurityLogListInput input);
}