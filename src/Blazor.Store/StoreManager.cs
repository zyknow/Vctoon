using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Volo.Abp.DependencyInjection;

namespace Blazor.Store;

public class StoreManager(
    IEnumerable<IStore> stores,
    ISessionStorageService sessionStorageService,
    ILocalStorageService localStorageService) : IScopedDependency
{
    private object lockObj = new();
    
    public Action ReRender;
    protected virtual bool IsInited { get; set; }
    
    public async Task InitStoresAsync()
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