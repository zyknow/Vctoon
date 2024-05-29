using Microsoft.AspNetCore.Components;
using Vctoon.Blazor.Client.Models;
using Vctoon.Blazor.Client.Providers;
using Vctoon.Blazor.Client.Services;

namespace Vctoon.Blazor.Client;

public abstract class ResourceViewBase<TResource> : VctoonComponentBase
{
    protected ResourceViewBase()
    {
        Info = new ResourceViewInfo<TResource>(LoadResourceAsync);
        
        Info.StateHasChanged += StateHasChanged;
    }
    
    [Inject] public NavigationManager NavigationManager { get; set; }
    
    [Inject] public IImageUrlProvider ImageUrlProvider { get; set; }
    
    [Inject] public DateTimeFormatService DateTimeFormatService { get; set; }
    
    public ResourceViewInfo<TResource> Info { get; set; }
    
    protected virtual void ResourceImageCheckChanged(TResource resource, bool isChecked)
    {
        if (isChecked)
        {
            Info.SelectedResources.AddIfNotContains(resource);
        }
        else
        {
            Info.SelectedResources.Remove(resource);
        }
    }
    
    protected virtual void OnResourceShiftClick(TResource resource)
    {
        if (Info.SelectedResources.IsNullOrEmpty())
        {
            return;
        }
        
        int firstIndex = Info.Resources.IndexOf(Info.SelectedResources.First());
        int lastIndex = Info.Resources.IndexOf(Info.SelectedResources.Last());
        
        int start = Math.Min(firstIndex, lastIndex);
        int end = Math.Max(firstIndex, lastIndex);
        
        Info.SelectedResources.Clear();
        
        for (int i = start; i <= end; i++)
        {
            Info.SelectedResources.Add(Info.Resources[i]);
        }
        
        StateHasChanged();
    }
    
    protected virtual async Task LoadResourceAsync(bool reload = false)
    {
        if (Info.IsLoading)
        {
            return;
        }
        
        await InvokeAsync(() =>
        {
            Info.IsLoading = true;
            StateHasChanged();
        });
        
        if (reload)
        {
            await InvokeAsync(async () =>
            {
                Info.Resources.Clear();
                Info.Total = 0;
                StateHasChanged();
            });
        }
        
        (List<TResource> Items, long TotalCount) listRes = await GetResourceListAsync();
        
        await InvokeAsync(async () =>
        {
            Info.IsLoading = false;
            Info.Total = listRes.TotalCount;
            
            Info.Resources.AddRange(listRes.Items);
            
            StateHasChanged();
        });
    }
    
    protected abstract Task<(List<TResource> Items, long TotalCount)> GetResourceListAsync();
    
    public override ValueTask DisposeAsync()
    {
        try
        {
            Info.StateHasChanged -= StateHasChanged;
        }
        catch (Exception e)
        {
        }
        
        return base.DisposeAsync();
    }
}