using Microsoft.AspNetCore.Components;
using Vctoon.Blazor.Client.Providers;
using Vctoon.Blazor.Client.Services;

namespace Vctoon.Blazor.Client;

public abstract class SelectResourceViewBase<TResource> : VctoonComponentBase
{
    [Inject] public NavigationManager NavigationManager { get; set; }
    
    [Inject] public IImageUrlProvider ImageUrlProvider { get; set; }
    
    [Inject] public DateTimeFormatService DateTimeFormatService { get; set; }
    
    protected List<TResource> Resources { get; set; } = [];
    
    protected long Total { get; set; }
    
    public virtual long LoadResourceCount { get; set; } = 50;
    
    protected List<TResource> SelectedResources { get; set; } = [];
    
    protected virtual void ResourceImageCheckChanged(TResource resource, bool isChecked)
    {
        if (isChecked)
        {
            SelectedResources.AddIfNotContains(resource);
        }
        else
        {
            SelectedResources.Remove(resource);
        }
    }
    
    protected virtual void OnResourceShiftClick(TResource resource)
    {
        if (SelectedResources.IsNullOrEmpty())
        {
            return;
        }
        
        int firstIndex = Resources.IndexOf(SelectedResources.First());
        int lastIndex = Resources.IndexOf(SelectedResources.Last());
        
        int start = Math.Min(firstIndex, lastIndex);
        int end = Math.Max(firstIndex, lastIndex);
        
        SelectedResources.Clear();
        
        for (int i = start; i <= end; i++)
        {
            SelectedResources.Add(Resources[i]);
        }
        
        StateHasChanged();
    }
    
    protected virtual async Task LoadResourceAsync(bool reload = false)
    {
        if (reload)
        {
            await InvokeAsync(async () =>
            {
                Resources.Clear();
                Total = 0;
                StateHasChanged();
            });
        }
        
        (List<TResource> Items, long TotalCount) listRes = await GetResourceListAsync();
        
        await InvokeAsync(async () =>
        {
            Total = listRes.TotalCount;
            Resources.AddRange(listRes.Items);
            StateHasChanged();
        });
    }
    
    protected abstract Task<(List<TResource> Items, long TotalCount)> GetResourceListAsync();
}