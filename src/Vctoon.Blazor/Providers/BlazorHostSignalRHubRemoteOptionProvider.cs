using System.Net;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.Extensions.Options;
using Vctoon.Blazor.Client.Providers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;

namespace Vctoon.Blazor.Providers;

public class BlazorHostSignalRHubRemoteOptionProvider(
    IOptions<AbpRemoteServiceOptions> remoteServiceOptions,
    IHttpContextAccessor httpContextAccessor)
    : ISignalRHubRemoteOptionProvider, ITransientDependency
{
    public void Config(HttpConnectionOptions options)
    {
        string? url = remoteServiceOptions.Value.RemoteServices.Default.BaseUrl;
        foreach (KeyValuePair<string, string> keyValuePair in httpContextAccessor.HttpContext.Request.Cookies)
        {
            options.Cookies.Add(new Uri(url), new Cookie(keyValuePair.Key, keyValuePair.Value));
        }
    }
}