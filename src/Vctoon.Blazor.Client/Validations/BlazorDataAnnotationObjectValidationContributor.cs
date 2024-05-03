using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client.Validations;

[Dependency(ReplaceServices = true)]
public class BlazorDataAnnotationObjectValidationContributor : DataAnnotationObjectValidationContributor
{
    public BlazorDataAnnotationObjectValidationContributor(IOptions<AbpValidationOptions> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    protected override void AddPropertyErrors(object validatingObject, PropertyDescriptor property, List<ValidationResult> errors)
    {
        var validationAttributes = property.Attributes.OfType<ValidationAttribute>().ToArray();
        if (validationAttributes.IsNullOrEmpty())
        {
            return;
        }

        var validationContext = new ValidationContext(validatingObject, ServiceProvider, null)
        {
            DisplayName = property.DisplayName,
            MemberName = property.Name
        };

        var attributeValidationResultProvider = ServiceProvider.GetRequiredService<AbpBlazorAttributeValidationResultProvider>();
        foreach (var attribute in validationAttributes)
        {
            var result = attributeValidationResultProvider.GetOrDefault(attribute, property.GetValue(validatingObject), validationContext);
            if (result != null)
            {
                errors.Add(result);
            }
        }
    }
}
