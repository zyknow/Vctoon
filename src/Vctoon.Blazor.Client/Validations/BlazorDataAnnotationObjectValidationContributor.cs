using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client.Validations;

[Dependency(ReplaceServices = true)]
public class BlazorDataAnnotationObjectValidationContributor(
    IOptions<AbpValidationOptions> options,
    IServiceProvider serviceProvider)
    : DataAnnotationObjectValidationContributor(options, serviceProvider)
{
    private const string PropertyLocalizationKeyPrefix = "DisplayName:";
    
    protected override void AddPropertyErrors(object validatingObject, PropertyDescriptor property, List<ValidationResult> errors)
    {
        var validationAttributes = property.Attributes.OfType<ValidationAttribute>().ToArray();
        if (validationAttributes.IsNullOrEmpty())
        {
            return;
        }
        
        var validationContext = new ValidationContext(validatingObject, ServiceProvider, null)
        {
            // TODO: localization
            DisplayName = property.DisplayName,
            //DisplayName = " ",
            MemberName = property.Name
        };
        
        var attributeValidationResultProvider = ServiceProvider.GetRequiredService<AbpBlazorAttributeValidationResultProvider>();
        foreach (var attribute in validationAttributes)
        {
            ValidationResult? result = attributeValidationResultProvider.GetOrDefault(attribute,
                property.GetValue(validatingObject),
                validationContext);
            if (result != null)
            {
                errors.Add(result);
            }
        }
    }
}