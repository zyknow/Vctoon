using Microsoft.AspNetCore.SignalR.Client;
using Vctoon.Blazor.Client.Providers;
using Volo.Abp.Http.Client;

namespace Vctoon.Blazor.Client.Hubs;

public abstract class VctoonHubBase(
    IRemoteServiceConfigurationProvider remoteServiceConfigurationProvider,
    ISignalRHubRemoteOptionProvider signalRHubRemoteOptionProvider
)
{
    private static readonly object _lockObject = new();
    public abstract string HubUrl { get; set; }
    public HubConnection? HubConnection { get; set; }
    
    public async Task StartAsync()
    {
        if (HubConnection != null)
        {
            return;
        }
        
        RemoteServiceConfiguration remote = await remoteServiceConfigurationProvider.GetConfigurationOrDefaultAsync();
        string hubUrl = @$"{remote.BaseUrl.EnsureEndsWith('/')}{HubUrl.TrimStart('/')}";
        
        lock (_lockObject)
        {
            if (HubConnection != null)
            {
                return;
            }
            
            HubConnection = new HubConnectionBuilder()
                .WithAutomaticReconnect()
                .WithUrl(hubUrl, signalRHubRemoteOptionProvider.Config)
                .Build();
        }
        
        ConfigHub(HubConnection);
        
        await HubConnection.StartAsync();
    }
    
    protected virtual void ConfigHub(HubConnection hubConnection)
    {
    }
}