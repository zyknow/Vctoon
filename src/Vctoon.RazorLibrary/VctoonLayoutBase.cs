using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FluentUI.AspNetCore.Components;
using Volo.Abp;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace Vctoon.RazorLibrary;

public abstract class VctoonLayoutBase : LayoutComponentBase, IDisposable
{
    private IAuthorizationService? _authorizationService;
    private IClock? _clock;
    private ICurrentTenant? _currentTenant;
    private ICurrentUser? _currentUser;
    private IDialogService? _dialogService;

    private Type? _localizationResource = typeof(DefaultResource);

    private IStringLocalizer? _localizer;
    private ILoggerFactory? _loggerFactory;
    private IMessageService? _messageService;

    private IObjectMapper? _objectMapper;

#nullable disable
    private AsyncServiceScope? _scope;

#nullable enable
    private IStringLocalizerFactory? _stringLocalizerFactory;
    private IToastService? _toastService;
    protected IStringLocalizerFactory StringLocalizerFactory => LazyGetRequiredService(ref _stringLocalizerFactory)!;

    protected IDialogService DialogService => LazyGetRequiredService(ref _dialogService)!;

    protected IMessageService MessageService => LazyGetRequiredService(ref _messageService)!;

    protected IToastService ToastService => LazyGetRequiredService(ref _toastService)!;

    protected IServiceProvider ScopedServices
    {
        get
        {
            if (ScopeFactory == null)
            {
                throw new InvalidOperationException("Services cannot be accessed before the component is initialized.");
            }

            ObjectDisposedException.ThrowIf(IsDisposed, (object) this);
            _scope.GetValueOrDefault();
            if (!_scope.HasValue)
            {
                _scope = new AsyncServiceScope?(ScopeFactory.CreateAsyncScope());
            }

            return _scope.Value.ServiceProvider;
        }
    }

    [Inject] private IServiceScopeFactory ScopeFactory { get; set; }

    /// <summary>
    /// Gets a value determining if the component and associated services have been disposed.
    /// </summary>
    protected bool IsDisposed { get; private set; }

    protected IStringLocalizer L
    {
        get
        {
            if (_localizer == null)
            {
                _localizer = CreateLocalizer();
            }

            return _localizer;
        }
    }

    protected Type? LocalizationResource
    {
        get => _localizationResource;
        set
        {
            _localizationResource = value;
            _localizer = null;
        }
    }

    protected ILogger Logger => _lazyLogger.Value;

    private Lazy<ILogger> _lazyLogger =>
        new(() => LoggerFactory?.CreateLogger(GetType().FullName!) ?? NullLogger.Instance, true);

    protected ILoggerFactory LoggerFactory => LazyGetRequiredService(ref _loggerFactory)!;

    protected IAuthorizationService AuthorizationService => LazyGetRequiredService(ref _authorizationService)!;

    protected ICurrentUser CurrentUser => LazyGetRequiredService(ref _currentUser)!;

    protected ICurrentTenant CurrentTenant => LazyGetRequiredService(ref _currentTenant)!;

    // protected IUiMessageService Message => LazyGetNonScopedRequiredService(ref _message)!;
    // private IUiMessageService? _message;
    //
    // protected IUiNotificationService Notify => LazyGetNonScopedRequiredService(ref _notify)!;
    // private IUiNotificationService? _notify;
    //
    // protected IUserExceptionInformer UserExceptionInformer => LazyGetNonScopedRequiredService(ref _userExceptionInformer)!;
    // private IUserExceptionInformer? _userExceptionInformer;
    //
    // protected IAlertManager AlertManager => LazyGetNonScopedRequiredService(ref _alertManager)!;
    // private IAlertManager? _alertManager;

    protected IClock Clock => LazyGetNonScopedRequiredService(ref _clock)!;

    // protected AlertList Alerts => AlertManager.Alerts;

    protected IObjectMapper ObjectMapper
    {
        get
        {
            if (_objectMapper != null)
            {
                return _objectMapper;
            }

            if (ObjectMapperContext == null)
            {
                return LazyGetRequiredService(ref _objectMapper)!;
            }

            return LazyGetRequiredService(
                typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext),
                ref _objectMapper
            )!;
        }
    }

    protected Type? ObjectMapperContext { get; set; }

    [Inject] protected IServiceProvider NonScopedServices { get; set; } = default!;

    // protected virtual async Task HandleErrorAsync(Exception exception)
    // {
    //     if (IsDisposed)
    //     {
    //         return;
    //     }
    //    
    //     Logger.LogException(exception);
    //     await InvokeAsync(async () =>
    //     {
    //         await UserExceptionInformer.InformAsync(new UserExceptionInformerContext(exception));
    //         StateHasChanged();
    //     });
    // }
    void IDisposable.Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        ref var local = ref _scope;
        if (local.HasValue)
        {
            local.GetValueOrDefault().Dispose();
        }

        _scope = new AsyncServiceScope?();
        Dispose(true);
        IsDisposed = true;
    }

    protected TService LazyGetRequiredService<TService>(ref TService reference)
    {
        return LazyGetRequiredService(typeof(TService), ref reference);
    }

    protected TRef LazyGetRequiredService<TRef>(Type serviceType, ref TRef reference)
    {
        if (reference == null)
        {
            reference = (TRef) ScopedServices.GetRequiredService(serviceType);
        }

        return reference;
    }

    protected TService? LazyGetService<TService>(ref TService? reference)
    {
        return LazyGetService(typeof(TService), ref reference);
    }

    protected TRef? LazyGetService<TRef>(Type serviceType, ref TRef? reference)
    {
        if (reference == null)
        {
            reference = (TRef?) ScopedServices.GetService(serviceType);
        }

        return reference;
    }

    protected TService LazyGetNonScopedRequiredService<TService>(ref TService reference)
    {
        return LazyGetNonScopedRequiredService(typeof(TService), ref reference);
    }

    protected TRef LazyGetNonScopedRequiredService<TRef>(Type serviceType, ref TRef reference)
    {
        if (reference == null)
        {
            reference = (TRef) NonScopedServices.GetRequiredService(serviceType);
        }

        return reference;
    }

    protected TService? LazyGetNonScopedService<TService>(ref TService? reference)
    {
        return LazyGetNonScopedService(typeof(TService), ref reference);
    }

    protected TRef? LazyGetNonScopedService<TRef>(Type serviceType, ref TRef? reference)
    {
        if (reference == null)
        {
            reference = (TRef?) NonScopedServices.GetService(serviceType);
        }

        return reference;
    }

    protected virtual IStringLocalizer CreateLocalizer()
    {
        if (LocalizationResource != null)
        {
            return StringLocalizerFactory.Create(LocalizationResource);
        }

        var localizer = StringLocalizerFactory.CreateDefaultOrNull();
        if (localizer == null)
        {
            throw new AbpException(
                $"Set {nameof(LocalizationResource)} or define the default localization resource type (by configuring the {nameof(AbpLocalizationOptions)}.{nameof(AbpLocalizationOptions.DefaultResourceType)}) to be able to use the {nameof(L)} object!");
        }

        return localizer;
    }

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
    }
}