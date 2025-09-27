using Volo.Abp.AspNetCore.SignalR;

namespace Vctoon.Hubs;

[Authorize]
public class DataChangedHub : AbpHub
{
}