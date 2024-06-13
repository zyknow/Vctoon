using System.Net;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Blazor.Client;

public class AuthorizationMessageHandler(IServiceProvider serviceProvider) : DelegatingHandler, ITransientDependency
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // TODO: can not work
            // ILoginService loginService = serviceProvider.GetRequiredService<ILoginService>();
            // await loginService.LoginAsync();
        }

        return response;
    }
}