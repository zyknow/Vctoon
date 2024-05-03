using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using Vctoon.Localization;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client.Validations;

public static class AbpEditContextDataAnnotationsExtensions
{
    private sealed class AbpDataAnnotationsEventSubscriptions : IDisposable
    {
        private static readonly ConcurrentDictionary<(Type ModelType, string FieldName), PropertyInfo> _propertyInfoCache = new ConcurrentDictionary<(Type, string), PropertyInfo>();

        private readonly EditContext _editContext;

        private readonly IServiceProvider _serviceProvider;
        private readonly IObjectValidator _objectValidator;

        private readonly ValidationMessageStore _messages;

        public AbpDataAnnotationsEventSubscriptions(EditContext editContext, IServiceProvider serviceProvider)
        {
            _editContext = editContext ?? throw new ArgumentNullException("editContext");
            _serviceProvider = serviceProvider;
            _objectValidator = _serviceProvider.GetRequiredService<IObjectValidator>();
            _messages = new ValidationMessageStore(_editContext);
            _editContext.OnFieldChanged += OnFieldChanged;
            _editContext.OnValidationRequested += OnValidationRequested;
            if (MetadataUpdater.IsSupported)
            {
                OnClearCache += ClearCache;
            }


        }

        // TODO: fix localization


        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Model types are expected to be defined in assemblies that do not get trimmed.")]
        private async void OnFieldChanged(object sender, FieldChangedEventArgs eventArgs)
        {
            FieldIdentifier fieldIdentifier = eventArgs.FieldIdentifier;

            if (TryGetValidatableProperty(in fieldIdentifier, out var propertyInfo))
            {
                var errors = await _objectValidator.GetErrorsAsync(_editContext.Model, propertyInfo.Name);
                _messages.Clear();
                if (errors.IsNullOrEmpty())
                    return;

                ValidationMessageStore messages = _messages;
                messages.Add(in fieldIdentifier, errors.First().ErrorMessage);
                Console.WriteLine(errors.First().ErrorMessage);

                _editContext.NotifyValidationStateChanged();
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Model types are expected to be defined in assemblies that do not get trimmed.")]
        private async void OnValidationRequested(object sender, ValidationRequestedEventArgs e)
        {

            var errors = await _objectValidator.GetErrorsAsync(_editContext.Model);

            _messages.Clear();



            foreach (ValidationResult item in errors)
            {
                bool flag = false;
                foreach (string memberName in item.MemberNames)
                {
                    flag = true;
                    ValidationMessageStore messages = _messages;
                    FieldIdentifier fieldIdentifier = _editContext.Field(memberName);
                    messages.Add(in fieldIdentifier, item.ErrorMessage);
                }

                if (!flag)
                {
                    ValidationMessageStore messages2 = _messages;
                    FieldIdentifier fieldIdentifier = new FieldIdentifier(_editContext.Model, string.Empty);
                    messages2.Add(in fieldIdentifier, item.ErrorMessage);
                }
            }

            _editContext.NotifyValidationStateChanged();
        }

        public void Dispose()
        {
            _messages.Clear();
            _editContext.OnFieldChanged -= OnFieldChanged;
            _editContext.OnValidationRequested -= OnValidationRequested;
            _editContext.NotifyValidationStateChanged();
            if (MetadataUpdater.IsSupported)
            {
                OnClearCache -= ClearCache;
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2080", Justification = "Model types are expected to be defined in assemblies that do not get trimmed.")]
        private static bool TryGetValidatableProperty(in FieldIdentifier fieldIdentifier, [NotNullWhen(true)] out PropertyInfo propertyInfo)
        {
            (Type, string) key = (fieldIdentifier.Model.GetType(), fieldIdentifier.FieldName);
            if (!_propertyInfoCache.TryGetValue(key, out propertyInfo))
            {
                propertyInfo = key.Item1.GetProperty(key.Item2);
                _propertyInfoCache[key] = propertyInfo;
            }

            return propertyInfo != null;
        }

        internal void ClearCache()
        {
            _propertyInfoCache.Clear();
        }
    }

    private static event Action? OnClearCache;
    //
    // 摘要:
    //     Enables DataAnnotations validation support for the Microsoft.AspNetCore.Components.Forms.EditContext.
    //
    //
    // 参数:
    //   editContext:
    //     The Microsoft.AspNetCore.Components.Forms.EditContext.
    //
    //   serviceProvider:
    //     The System.IServiceProvider to be used in the System.ComponentModel.DataAnnotations.ValidationContext.
    //
    //
    // 返回结果:
    //     A disposable object whose disposal will remove DataAnnotations validation support
    //     from the Microsoft.AspNetCore.Components.Forms.EditContext.
    public static IDisposable EnableAbpDataAnnotationsValidation(this EditContext editContext, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, "serviceProvider");
        return new AbpDataAnnotationsEventSubscriptions(editContext, serviceProvider);
    }

    private static void ClearCache(Type[] _)
    {
        AbpEditContextDataAnnotationsExtensions.OnClearCache?.Invoke();
    }
}
