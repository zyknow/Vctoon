using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Vctoon.Blazor.Client.Validations;

public static class ValidationAttributeHelper
{
    private static readonly PropertyInfo _validationAttributeErrorMessageStringProperty = typeof(ValidationAttribute)
        .GetProperty("ErrorMessageString", BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    private static readonly PropertyInfo _validationAttributeCustomErrorMessageSetProperty = typeof(ValidationAttribute)
        .GetProperty("CustomErrorMessageSet", BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    public static void SetDefaultErrorMessage(ValidationAttribute validationAttribute)
    {
        if (validationAttribute is StringLengthAttribute stringLength && stringLength.MinimumLength != 0)
        {
            bool? customErrorMessageSet =
                _validationAttributeCustomErrorMessageSetProperty.GetValue(validationAttribute) as bool?;
            if (customErrorMessageSet != true)
            {
                validationAttribute.ErrorMessage =
                    "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.";
                return;
            }
        }
        
        try
        {
            string? errorMessageString = _validationAttributeErrorMessageStringProperty.GetValue(validationAttribute) as string;
            validationAttribute.ErrorMessage = errorMessageString;
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}