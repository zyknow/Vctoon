using Microsoft.AspNetCore.Components;

namespace Blazor.Store;

public abstract class BlazorStoreComponentBase : ComponentBase, IAsyncDisposable
{
    [Inject] protected StoreManager StoreManager { get; set; }
    
    public async ValueTask DisposeAsync()
    {
        StoreManager.ReRender -= StateHasChanged;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            StoreManager.ReRender += StateHasChanged;
        }
    }
}