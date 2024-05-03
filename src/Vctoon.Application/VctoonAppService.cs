using Vctoon.Localization.Vctoon;

namespace Vctoon;

/* Inherit your application services from this class.
 */
public abstract class VctoonAppService : ApplicationService
{
    protected VctoonAppService()
    {
        LocalizationResource = typeof(VctoonResource);
    }
}