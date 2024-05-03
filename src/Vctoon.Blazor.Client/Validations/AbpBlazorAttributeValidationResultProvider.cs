using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Vctoon.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client.Validations;

public class AbpBlazorAttributeValidationResultProvider : DefaultAttributeValidationResultProvider
{
    private readonly IOptions<AbpLocalizationOptions> localizationOptions;
    private readonly IStringLocalizerFactory stringLocalizerFactory;

    public AbpBlazorAttributeValidationResultProvider(
               IOptions<AbpLocalizationOptions> localizationOptions,
                      IStringLocalizerFactory stringLocalizerFactory)
    {
        this.localizationOptions = localizationOptions;
        this.stringLocalizerFactory = stringLocalizerFactory;
    }

    public override ValidationResult? GetOrDefault(ValidationAttribute validationAttribute, object? validatingObject, ValidationContext validationContext)
    {
        // TODO: Cache defaultResourceName?
        var defaultResourceName = localizationOptions.Value.DefaultResourceType.GetCustomAttribute<LocalizationResourceNameAttribute>()?.Name;
        var resourceSource = localizationOptions.Value.Resources.GetOrDefault(defaultResourceName);
        if (resourceSource == null)
        {
            return base.GetOrDefault(validationAttribute, validatingObject, validationContext);
        }

        if (validationAttribute.ErrorMessage == null)
        {
            ValidationAttributeHelper.SetDefaultErrorMessage(validationAttribute);
        }

        if (validationAttribute.ErrorMessage != null)
        {
            validationAttribute.ErrorMessage = stringLocalizerFactory.Create(typeof(VctoonResource))[validationAttribute.ErrorMessage];
        }

        return base.GetOrDefault(validationAttribute, validatingObject, validationContext);
    }

}
