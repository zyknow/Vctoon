using Microsoft.AspNetCore.Http.Connections.Client;

namespace Vctoon.Blazor.Client.Providers;

public interface ISignalRHubRemoteOptionProvider
{
    void Config(HttpConnectionOptions options);
}