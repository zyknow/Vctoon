using AsyncKeyedLock;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Volo.Abp.DependencyInjection;

namespace Blazor.Store;

public class StoreManager(
    IEnumerable<IStore> stores,
    ISessionStorageService sessionStorageService,
    ILocalStorageService localStorageService) : IScopedDependency
{
    private readonly AsyncKeyedLocker<string> _initAsyncKeyedLocker = new(o =>
    {
        o.PoolSize = 20; // this is NOT a concurrency limit
        o.PoolInitialFill = 1;
    });
    
    public Action ReRender;
    protected virtual bool IsInited { get; set; }
    
    public virtual async Task InitStoresAsync()
    {
        if (IsInited)
        {
            return;
        }
        
        using (await _initAsyncKeyedLocker.LockAsync(nameof(StoreManager)))
        {
            if (IsInited)
            {
                return;
            }
            
            foreach (IStore store in stores)
            {
                await store.InitReRenderAsync(sessionStorageService, localStorageService);
                store.ReRender += () => ReRender?.Invoke();
            }
            
            IsInited = true;
            ReRender?.Invoke();
        }
    }
}