using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Vctoon.Blazor.Client.Components.Settings;

public partial class AppSettingPanelContent(DesignThemeModes themeMode, OfficeColor? themeColor = default) : ObservableObject
{
    [ObservableProperty] private OfficeColor? _themeColor = themeColor;

    [ObservableProperty] private DesignThemeModes _themeMode = themeMode;
}