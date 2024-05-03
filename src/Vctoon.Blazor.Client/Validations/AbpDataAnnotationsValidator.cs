﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;

namespace Vctoon.Blazor.Client.Validations;

public class AbpDataAnnotationsValidator : ComponentBase, IDisposable
{
    private IDisposable _subscriptions;

    private EditContext _originalEditContext;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; }

    [Inject]
    public IStringLocalizer<AbpValidationResource> Localizer { get; set; }

    [Inject]
    public IMethodInvocationValidator MethodInvocationValidator { get; set; }

    [Inject]
    public IObjectValidator ObjectValidator { get; set; }

    protected override void OnInitialized()
    {
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException("DataAnnotationsValidator requires a cascading parameter of type EditContext. For example, you can use DataAnnotationsValidator inside an EditForm.");
        }
        _subscriptions = CurrentEditContext.EnableAbpDataAnnotationsValidation(ServiceProvider);
        _originalEditContext = CurrentEditContext;

        // 订阅验证状态改变事件
        CurrentEditContext.OnValidationStateChanged += OnValidationStateChanged;
        CurrentEditContext.OnFieldChanged += OnFieldChanged;
        CurrentEditContext.OnValidationRequested += OnValidationRequested;
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
    }

    private async void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        //await ObjectValidator.ValidateAsync(CurrentEditContext.Model);
    }

    private async void OnValidationStateChanged(object sender, ValidationStateChangedEventArgs e)
    {
        //await ObjectValidator.ValidateAsync(CurrentEditContext.Model);
    }

    protected override void OnParametersSet()
    {
        if (CurrentEditContext != _originalEditContext)
        {
            throw new InvalidOperationException($"{GetType()} does not support changing the {"EditContext"} dynamically.");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    void IDisposable.Dispose()
    {
        _subscriptions?.Dispose();
        _subscriptions = null;
        Dispose(disposing: true);
    }
}
