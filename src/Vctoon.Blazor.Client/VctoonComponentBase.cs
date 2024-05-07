using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Vctoon.Localization.Vctoon;

namespace Vctoon.Blazor.Client;

public abstract class VctoonComponentBase : BlazorStoreComponentBase
{
    [Inject] protected IStringLocalizer<VctoonResource> L { get; set; }
}