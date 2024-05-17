using Vctoon.Blazor.Client;

namespace Vctoon.Blazor;

public class BlazorHostHttpRequestAccessor(IConfiguration configuration)
    : IBlazorHttpRequestAccessor
{
    public string RemoteUrl { get; } = configuration["RemoteServices:Default:BaseUrl"];
}