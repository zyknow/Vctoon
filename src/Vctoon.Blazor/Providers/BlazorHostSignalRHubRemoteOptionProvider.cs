using System.Net;
using Microsoft.AspNetCore.Http.Connections.Client;
using Vctoon.Blazor.Client.Providers;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Blazor.Providers;

public class BlazorHostSignalRHubRemoteOptionProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    : ISignalRHubRemoteOptionProvider, ITransientDependency
{
    // public string GetRemoteServiceUrl()
    // {
    //     var url =  configuration["RemoteServices:Default:BaseUrl"];
    //     return url;
    // }
    
    public void Config(HttpConnectionOptions options)
    {
        string? url = configuration["RemoteServices:Default:BaseUrl"];
        Uri uri = new Uri(url);
        foreach (KeyValuePair<string, string> keyValuePair in httpContextAccessor.HttpContext.Request.Cookies)
        {
            options.Cookies.Add(uri, new Cookie(keyValuePair.Key, keyValuePair.Value));
        }
    }
}