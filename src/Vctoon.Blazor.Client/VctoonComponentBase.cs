using Blazor.Store;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Vctoon.Localization;

namespace Vctoon.Blazor.Client;

public abstract class VctoonComponentBase : BlazorStoreComponentBase
{
    [Inject]
    protected IStringLocalizer<VctoonResource> L { get; set; }
}