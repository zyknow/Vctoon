using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client.Validations;

[Dependency(ReplaceServices = true)]
public class AbpBlazorAttributeValidationResultProvider : DefaultAttributeValidationResultProvider
{
    private readonly IOptions<AbpLocalizationOptions> _localizationOptions;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;
    
    public AbpBlazorAttributeValidationResultProvider(
        IOptions<AbpLocalizationOptions> localizationOptions,
        IStringLocalizerFactory stringLocalizerFactory)
    {
        _localizationOptions = localizationOptions;
        _stringLocalizerFactory = stringLocalizerFactory;
    }
    
    public override ValidationResult? GetOrDefault(ValidationAttribute validationAttribute, object? validatingObject,
        ValidationContext validationContext)
    {
        // TODO: Cache defaultResourceName?
        string? defaultResourceName = _localizationOptions.Value.DefaultResourceType
            .GetCustomAttribute<LocalizationResourceNameAttribute>()?.Name;
        LocalizationResourceBase? resourceSource = _localizationOptions.Value.Resources.GetOrDefault(defaultResourceName);
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
            validationAttribute.ErrorMessage =
                _stringLocalizerFactory.Create(_localizationOptions.Value.DefaultResourceType)[validationAttribute.ErrorMessage];
        }
        
        ValidationResult? res = base.GetOrDefault(validationAttribute, validatingObject, validationContext);
        
        return res;
    }
}