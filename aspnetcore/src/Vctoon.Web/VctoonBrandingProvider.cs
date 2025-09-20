using Microsoft.Extensions.Localization;
using Vctoon.Localization.Vctoon;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Vctoon.Web;

[Dependency(ReplaceServices = true)]
public class VctoonBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<VctoonResource> _localizer;

    public VctoonBrandingProvider(IStringLocalizer<VctoonResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}