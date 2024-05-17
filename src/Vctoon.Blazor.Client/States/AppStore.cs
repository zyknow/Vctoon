using CommunityToolkit.Mvvm.ComponentModel;
using Vctoon.Blazor.Client.Enums;

namespace Vctoon.Blazor.Client.States;

[SessionStorage]
public partial class AppStore : StateBase<AppStore>
{
    [ObservableProperty] private bool _activePopoverAutoOpen = true;
    
    [ObservableProperty] private string _currentCulture;
    
    [ObservableProperty] private double _imageScale = 1;
    
    [ObservableProperty] private LibraryResourceDisplayMode _libraryResourceDisplayMode = LibraryResourceDisplayMode.Grid;
    
    [ObservableProperty] private bool _navMenuExpanded = true;
}