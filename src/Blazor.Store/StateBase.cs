using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using AsyncKeyedLock;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CommunityToolkit.Mvvm.ComponentModel;
using Volo.Abp.DependencyInjection;

namespace Blazor.Store;

public abstract class StateBase<TStore> : ObservableObject, IStore, IScopedDependency where TStore : StateBase<TStore>
{
    private readonly AsyncKeyedLocker<string> _initAsyncKeyedLocker = new(o =>
    {
        o.PoolSize = 20; // this is NOT a concurrency limit
        o.PoolInitialFill = 1;
    });
    
    private readonly Type _storeType = typeof(TStore);
    
    private BlazorStoreType? _currentStoreType;
    
    private List<PropertyInfo> _saveProperties = [];
    
    protected bool Inited { get; set; }
    
    protected virtual string StoreKey { get; set; } = typeof(TStore).Name?.Replace("Store", "");
    public event Action ReRender;
    
    
    public async Task InitReRenderAsync(ISessionStorageService sessionStorageService, ILocalStorageService localStorageService)
    {
        if (Inited)
        {
            return;
        }
        
        using (await _initAsyncKeyedLocker.LockAsync(StoreKey))
        {
            if (Inited)
            {
                return;
            }
            
            string[] ignorePropertyNames = _storeType.GetCustomAttribute<IgnorePropertyStoreAttribute>()?.Names ?? [];
            string[] ignoreDeepWatchPropertyNames =
                _storeType.GetCustomAttribute<IgnorePropertyDeepWatchStoreAttribute>()?.Names ?? [];
            
            _saveProperties =
                _storeType.GetProperties().Where(x => !ignorePropertyNames.Contains(x.Name)).ToList();
            
            List<PropertyInfo> deppWatchProperties = _storeType.GetProperties()
                .Where(x => !ignoreDeepWatchPropertyNames.Contains(x.Name))
                .Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                .Where(x => x.PropertyType.IsAssignableFrom(typeof(INotifyPropertyChanged)))
                .ToList();
            
            List<string> savePropertyNames = _saveProperties.Select(x => x.Name).ToList();
            
            BlazorStoreType currentStoreType = GetCurrentStoreType();
            
            await LoadStateAsync(sessionStorageService, localStorageService);
            ReRender?.Invoke();
            
            PropertyChanged += async (sender, args) =>
            {
                ReRender?.Invoke();
                
                if (currentStoreType == BlazorStoreType.None)
                {
                    return;
                }
                
                if (args.PropertyName is not null && savePropertyNames.Contains(args.PropertyName))
                {
                    PropertyInfo? property = _saveProperties.FirstOrDefault(x => x.Name == args.PropertyName);
                    object? value = property.GetValue(this);
                    
                    if (currentStoreType == BlazorStoreType.SessionStorage)
                    {
                        // if (value is null)
                        //     await sessionStorageService.RemoveItemAsync($"{StoreKey}.{args.PropertyName}");
                        // else
                        await sessionStorageService.SetItemAsync($"{StoreKey}.{args.PropertyName}", value);
                    }
                    else if (currentStoreType == BlazorStoreType.LocalStorage)
                    {
                        // if (value is null)
                        //     await localStorageService.RemoveItemAsync($"{StoreKey}.{args.PropertyName}");
                        // else
                        await localStorageService.SetItemAsync($"{StoreKey}.{args.PropertyName}", value);
                    }
                }
            };
            
            // handle deep property change
            foreach (PropertyInfo property in deppWatchProperties)
            {
                INotifyPropertyChanged? notifyPropertyChanged = property.GetValue(this) as INotifyPropertyChanged;
                if (notifyPropertyChanged is null)
                {
                    continue;
                }
                
                notifyPropertyChanged.PropertyChanged += (s, e) => { OnPropertyChanged(property.Name); };
            }
            
            await OnInitReRenderAsync();
            Inited = true;
        }
    }
    
    public async Task LoadStateAsync(ISessionStorageService sessionStorageService, ILocalStorageService localStorageService)
    {
        if (_saveProperties.IsNullOrEmpty())
        {
            return;
        }
        
        BlazorStoreType currentStoreType = GetCurrentStoreType();
        
        if (currentStoreType == BlazorStoreType.None)
        {
            return;
        }
        
        
        foreach (PropertyInfo saveProperty in _saveProperties)
        {
            string propertyName = saveProperty.Name;
            Type propertyType = saveProperty.PropertyType;
            
            
            string? stringValue = null;
            
            if (currentStoreType == BlazorStoreType.SessionStorage)
            {
                stringValue = await sessionStorageService.GetItemAsStringAsync($"{StoreKey}.{propertyName}");
            }
            else if (currentStoreType == BlazorStoreType.LocalStorage)
            {
                stringValue = await localStorageService.GetItemAsStringAsync($"{StoreKey}.{propertyName}");
            }
            
            if (stringValue is null)
            {
                continue;
            }
            
            object? obj = GetObjectValue(stringValue, propertyType);
            
            if (obj is not null)
            {
                saveProperty.SetValue(this, obj);
            }
        }
    }
    
    
    protected virtual async Task OnInitReRenderAsync()
    {
    }
    
    private BlazorStoreType GetCurrentStoreType()
    {
        if (_currentStoreType is not null)
        {
            return _currentStoreType.Value;
        }
        
        
        bool isSessionStorage = _storeType.GetCustomAttribute<SessionStorageAttribute>() is not null;
        bool isLocalStorage = _storeType.GetCustomAttribute<LocalStorageAttribute>() is not null;
        
        if (isSessionStorage && isLocalStorage)
        {
            throw new InvalidOperationException("Store can't be both SessionStorage and LocalStorage");
        }
        
        if (isSessionStorage)
        {
            return BlazorStoreType.SessionStorage;
        }
        
        if (isLocalStorage)
        {
            return BlazorStoreType.LocalStorage;
        }
        
        return BlazorStoreType.None;
    }
    
    private object? GetObjectValue(string? stringValue, Type type)
    {
        if (stringValue == null && type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        
        if (stringValue == null)
        {
            return null;
        }
        
        
        string text = stringValue;
        if (string.IsNullOrWhiteSpace(text))
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
        
        try
        {
            return JsonSerializer.Deserialize(text, type);
        }
        catch (JsonException ex) when (ex.Path == "$" && type == typeof(string))
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}