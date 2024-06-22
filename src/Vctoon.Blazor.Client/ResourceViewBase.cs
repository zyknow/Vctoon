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

    [SupplyParameterFromQuery] public int Page { get; set; }

    [SupplyParameterFromQuery]
    public int PageSize
    {
        get => Info.LoadResourceCount;
        set => Info.LoadResourceCount = value;
    }

    [Inject] public NavigationManager NavigationManager { get; set; }

    [Inject] public IImageUrlProvider ImageUrlProvider { get; set; }

    [Inject] public DateTimeFormatService DateTimeFormatService { get; set; }

    public ResourceViewInfo<TResource> Info { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (Page <= 1)
            {
                Page = 1;
            }

            if (PageSize <= 0)
            {
                PageSize = 50;
            }
        }

        base.OnAfterRender(firstRender);
    }

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

        if (firstIndex == -1 || lastIndex == -1)
        {
            return;
        }


        int start = Math.Min(firstIndex, lastIndex);
        int end = Math.Max(firstIndex, lastIndex);

        Info.SelectedResources.Clear();

        for (int i = start; i <= end; i++)
        {
            Info.SelectedResources.Add(Info.Resources[i]);
        }

        StateHasChanged();
    }

    protected virtual async Task LoadResourceAsync()
    {
        if (Info.IsLoading)
        {
            return;
        }

        Info.IsLoading = true;
        (List<TResource> Items, long TotalCount) listRes = await GetResourceListAsync();
        Info.IsLoading = false;
        Info.Total = listRes.TotalCount;
        Info.Resources = listRes.Items;

        StateHasChanged();
    }

    protected abstract Task<(List<TResource> Items, long TotalCount)> GetResourceListAsync();

    protected void Reset()
    {
        Info.SelectedResources = [];
        Info.Resources = [];
        Info.Total = 0;
        Info.IsLoading = false;
    }

    protected virtual async Task OnPaginationDataChanged(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
        await NavigationToPageAsync(page, pageSize);
    }

    protected virtual async Task NavigationToPageAsync(int page, int pageSize = 50)
    {
        string url = NavigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["page"] = page,
            ["pageSize"] = pageSize
        });
        await LoadResourceAsync();
        NavigationManager.NavigateTo(url, replace: true);
    }

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