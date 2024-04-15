using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Vctoon;

[Dependency(ReplaceServices = true)]
public class VctoonBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Vctoon";
}