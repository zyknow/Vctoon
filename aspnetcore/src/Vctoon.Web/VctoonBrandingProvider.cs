using Microsoft.Extensions.Localization;
using Vctoon.Localization.Vctoon;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Vctoon.Web;

[Dependency(ReplaceServices = true)]
public class VctoonBrandingProvider(IStringLocalizer<VctoonResource> localizer) : DefaultBrandingProvider
{
    public override string AppName => localizer["AppName"];
}