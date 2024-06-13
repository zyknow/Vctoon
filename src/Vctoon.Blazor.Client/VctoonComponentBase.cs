using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Vctoon.Localization.Vctoon;
using Volo.Abp.Users;

namespace Vctoon.Blazor.Client;

public abstract class VctoonComponentBase : BlazorStoreComponentBase
{
    [Inject] protected IStringLocalizer<VctoonResource> L { get; set; }

    [Inject] protected ICurrentUser CurrentUser { get; set; }
}