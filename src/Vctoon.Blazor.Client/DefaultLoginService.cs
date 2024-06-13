using Microsoft.AspNetCore.Components;
using Vctoon.Blazor.Client.Services;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Blazor.Client;

public class DefaultLoginService : ILoginService, ITransientDependency
{
    [Inject] public NavigationManager NavigationManager { get; set; }

    public async Task LogoutAsync()
    {
        NavigationManager.NavigateTo("Account/Logout", true);
    }

    public async Task LoginAsync()
    {
        NavigationManager.NavigateTo("Account/Login", true);
    }
}