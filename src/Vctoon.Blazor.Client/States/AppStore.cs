using Blazor.Store;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vctoon.Blazor.Client.States;

[SessionStorage]
public partial class AppStore : StateBase<AppStore>
{
    [ObservableProperty]
    string currentCulture;
}