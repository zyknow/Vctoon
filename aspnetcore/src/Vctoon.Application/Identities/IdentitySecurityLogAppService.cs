using Vctoon.Identities.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Vctoon.Identities;

[Authorize]
public class IdentitySecurityLogAppService(IIdentitySecurityLogRepository identitySecurityLogRepository)
    : IdentityAppServiceBase, IIdentitySecurityLogAppService
{
    [Authorize(IdentityPermissions.Users.ManagePermissions)]
    public virtual async Task<PagedResultDto<IdentitySecurityLogDto>> GetListAsync(
        GetIdentitySecurityLogListInput input)
    {
        List<IdentitySecurityLog>? securityLogs = await identitySecurityLogRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.StartTime,
            input.EndTime,
            input.ApplicationName,
            input.Identity,
            input.ActionName,
            userName: input.UserName,
            clientId: input.ClientId,
            correlationId: input.CorrelationId
        );

        var totalCount = await identitySecurityLogRepository.GetCountAsync(
            input.StartTime,
            input.EndTime,
            input.ApplicationName,
            input.Identity,
            input.ActionName,
            userName: input.UserName,
            clientId: input.ClientId,
            correlationId: input.CorrelationId
        );

        var securityLogDtos =
            ObjectMapper.Map<List<IdentitySecurityLog>, List<IdentitySecurityLogDto>>(securityLogs);
        return new PagedResultDto<IdentitySecurityLogDto>(totalCount, securityLogDtos);
    }

    public virtual async Task<PagedResultDto<IdentitySecurityLogDto>> GetCurrentUserListAsync(
        GetIdentitySecurityLogListInput input)
    {
        List<IdentitySecurityLog>? securityLogs = await identitySecurityLogRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.StartTime,
            input.EndTime,
            input.ApplicationName,
            input.Identity,
            input.ActionName,
            CurrentUser.GetId(),
            input.UserName,
            input.ClientId,
            input.CorrelationId
        );

        var totalCount = await identitySecurityLogRepository.GetCountAsync(
            input.StartTime,
            input.EndTime,
            input.ApplicationName,
            input.Identity,
            input.ActionName,
            CurrentUser.GetId(),
            input.UserName,
            input.ClientId,
            input.CorrelationId
        );

        var securityLogDtos =
            ObjectMapper.Map<List<IdentitySecurityLog>, List<IdentitySecurityLogDto>>(securityLogs);
        return new PagedResultDto<IdentitySecurityLogDto>(totalCount, securityLogDtos);
    }
}