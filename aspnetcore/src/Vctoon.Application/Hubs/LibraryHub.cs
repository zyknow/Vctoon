using Vctoon.Permissions;
using Volo.Abp.AspNetCore.SignalR;

namespace Vctoon.Hubs;

[Authorize(Policy = VctoonPermissions.Library.Default)]
public class LibraryHub : AbpHub
{
    public LibraryHub()
    {
    }
}