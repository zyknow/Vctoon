using CommunityToolkit.Mvvm.ComponentModel;

namespace Vctoon.Blazor.Client.States;

[SessionStorage]
public partial class AppStore : StateBase<AppStore>
{
    [ObservableProperty]
    private string _currentCulture;
    
    [ObservableProperty]
    private bool navMenuExpanded = true;
}