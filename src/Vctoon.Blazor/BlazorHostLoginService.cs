using Microsoft.AspNetCore.Components;
using Vctoon.Blazor.Client.Services;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Blazor;

public class BlazorHostLoginService(NavigationManager navigationManager) : ILoginService, ITransientDependency
{
    public async Task LogoutAsync()
    {
        navigationManager.NavigateTo("Account/Logout", true);
    }
}