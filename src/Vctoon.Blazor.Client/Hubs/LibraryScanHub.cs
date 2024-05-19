using Microsoft.AspNetCore.SignalR.Client;
using Vctoon.Blazor.Client.Providers;
using Vctoon.Hubs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;

namespace Vctoon.Blazor.Client.Hubs;

public class LibraryScanHub(
    IRemoteServiceConfigurationProvider remoteServiceConfigurationProvider,
    ISignalRHubRemoteOptionProvider signalRHubRemoteOptionProvider)
    : VctoonHubBase(remoteServiceConfigurationProvider, signalRHubRemoteOptionProvider), IScopedDependency, IAsyncDisposable
{
    public override string HubUrl { get; set; } = "/signalr-hubs/library-scan";
    
    
    public async ValueTask DisposeAsync()
    {
        OnScanning = null;
        OnScanned = null;
        if (HubConnection != null)
        {
            await HubConnection.DisposeAsync();
        }
    }
    
    public event Action<LibraryScanMessage> OnScanning;
    public event Action<Guid> OnScanned;
    
    protected override void ConfigHub(HubConnection hubConnection)
    {
        hubConnection.On<LibraryScanMessage>(HubEventConst.Library.OnScanning, (msg) => { OnScanning?.Invoke(msg); });
        hubConnection.On<Guid>(HubEventConst.Library.OnScanned, (libraryId) => { OnScanned?.Invoke(libraryId); });
    }
}