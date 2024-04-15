using System;
using System.Collections.Generic;
using System.Text;
using Vctoon.Localization;
using Volo.Abp.Application.Services;

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