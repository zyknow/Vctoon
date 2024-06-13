using Vctoon.Identities.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Identities;

public interface IIdentitySecurityLogAppService : IApplicationService
{
    Task<PagedResultDto<IdentitySecurityLogDto>> GetListAsync(
        GetIdentitySecurityLogListInput input);

    Task<PagedResultDto<IdentitySecurityLogDto>> GetCurrentUserListAsync(
        GetIdentitySecurityLogListInput input);
}