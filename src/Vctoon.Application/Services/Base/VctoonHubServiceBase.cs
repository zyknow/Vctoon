using Microsoft.AspNetCore.SignalR;
using Vctoon.Hubs;

namespace Vctoon.Services.Base;

public abstract class VctoonHubServiceBase(IHubContext<LibraryScanHub> libraryScanHub) : VctoonService
{
    public virtual async Task SendLibraryScanMessageAsync(Guid libraryId, string title, string? message = null)
    {
        await libraryScanHub.Clients.All.SendAsync(HubEventConst.Library.OnScanning, new LibraryScanMessage()
        {
            LibraryId = libraryId,
            Title = title,
            Message = message
        });
    }
    
    public virtual async Task SendLibraryScannedAsync(Guid libraryId)
    {
        await libraryScanHub.Clients.All.SendAsync(HubEventConst.Library.OnScanned, libraryId);
    }
}