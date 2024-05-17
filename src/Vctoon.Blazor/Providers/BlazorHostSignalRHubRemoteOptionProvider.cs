using System.Net;
using Microsoft.AspNetCore.Http.Connections.Client;
using Vctoon.Blazor.Client;
using Vctoon.Blazor.Client.Providers;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Blazor.Providers;

public class BlazorHostSignalRHubRemoteOptionProvider(
    IBlazorHttpRequestAccessor blazorHttpRequestAccessor,
    IHttpContextAccessor httpContextAccessor)
    : ISignalRHubRemoteOptionProvider, ITransientDependency
{
    // public string GetRemoteServiceUrl()
    // {
    //     var url =  configuration["RemoteServices:Default:BaseUrl"];
    //     return url;
    // }
    
    public void Config(HttpConnectionOptions options)
    {
        string? url = blazorHttpRequestAccessor.RemoteUrl;
        foreach (KeyValuePair<string, string> keyValuePair in httpContextAccessor.HttpContext.Request.Cookies)
        {
            options.Cookies.Add(new Uri(url), new Cookie(keyValuePair.Key, keyValuePair.Value));
        }
    }
}