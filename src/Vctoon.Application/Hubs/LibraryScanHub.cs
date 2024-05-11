using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.SignalR;

namespace Vctoon.Hubs;

[Authorize(Policy = VctoonPermissions.Library.Default)]
public class LibraryScanHub : AbpHub
{
    public LibraryScanHub()
    {
    }
}