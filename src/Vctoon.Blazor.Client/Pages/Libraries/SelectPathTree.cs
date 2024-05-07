namespace Vctoon.Blazor.Client.Pages.Libraries;

public class SelectPath(string path)
{
    public string Path { get; set; } = path;
    public List<SelectPathTree> Children { get; set; } = [];
}

public class SelectPathTree(string path) : SelectPath(path)
{
    public string Path { get; set; } = path;
    public List<SelectPathTree> Children { get; set; } = [];
}