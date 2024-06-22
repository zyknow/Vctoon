using CommunityToolkit.Mvvm.ComponentModel;

namespace Vctoon.Blazor.Client.Models;

public class ResourceViewInfo<TResource>(Func<Task> loadResourceFunc)
{
    public Action StateHasChanged;

    public SoftInfo Soft { get; set; } = new();

    public List<TResource> Resources { get; set; } = [];

    public long Total { get; set; }

    public int LoadResourceCount { get; set; } = 50;

    public List<TResource> SelectedResources { get; set; } = [];

    public bool IsLoading { get; set; }

    public Task LoadResourceAsync()
    {
        return loadResourceFunc();
    }
}

public class SoftInfo : ObservableObject
{
    public string Field { get; set; } = "Id";
    public string SoftBy { get; set; } = "desc";

    public override string ToString()
    {
        if (Field.IsNullOrEmpty() || SoftBy.IsNullOrEmpty())
        {
            return "";
        }

        return $"{Field} {SoftBy}";
    }
}