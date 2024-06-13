using CommunityToolkit.Mvvm.ComponentModel;
using Vctoon.Libraries.Dtos;

namespace Vctoon.Blazor.Client.States;

[SessionStorage]
[IgnorePropertyStore(nameof(Libraries), nameof(Tags))]
public partial class UserStore(ILibraryAppService libraryAppService, ITagAppService tagAppService) : StateBase<UserStore>
{
    [ObservableProperty] private List<LibraryDto> _libraries = new();

    [ObservableProperty] private List<TagDto> _tags = new();

    public async Task LoadLibrariesAsync()
    {
        Libraries = await libraryAppService.GetCurrentUserLibraryListAsync();
    }

    public async Task LoadTagsAsync()
    {
        Tags = await tagAppService.GetAllTagAsync();
    }
}