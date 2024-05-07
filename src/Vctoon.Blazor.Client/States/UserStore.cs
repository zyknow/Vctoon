using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Vctoon.Libraries;
using Vctoon.Libraries.Dtos;

namespace Vctoon.Blazor.Client.States;

[SessionStorage]
[IgnorePropertyStore(nameof(Libraries))]
public partial class UserStore(ILibraryAppService libraryAppService) : StateBase<UserStore>
{
    [ObservableProperty]
    private ObservableCollection<LibraryDto> _libraries = new();
    
    public async Task LoadLibrariesAsync()
    {
        Libraries = new ObservableCollection<LibraryDto>(await libraryAppService.GetCurrentUserLibraryListAsync());
    }
}