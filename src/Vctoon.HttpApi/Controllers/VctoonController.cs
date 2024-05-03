using Vctoon.Localization.Vctoon;
using Volo.Abp.AspNetCore.Mvc;

namespace Vctoon.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class VctoonController : AbpControllerBase
{
    protected VctoonController()
    {
        LocalizationResource = typeof(VctoonResource);
    }
}