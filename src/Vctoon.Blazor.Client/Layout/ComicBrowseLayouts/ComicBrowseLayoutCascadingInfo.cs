using Vctoon.Comics.Dtos;
using Vctoon.Libraries.Dtos;

namespace Vctoon.Blazor.Client.Layout.ComicBrowseLayouts;

public class ComicBrowseLayoutCascadingInfo
{
    public string Title { get; set; }

    public ComicDto? Comic { get; set; }

    public List<ImageFileDto> ImageFiles { get; set; } = [];

    public Action StateHasChanged { get; set; }
}