using Blazored.LocalStorage;
using Blazored.SessionStorage;

namespace Blazor.Store;

public interface IStore
{
    event Action ReRender;
    Task InitReRenderAsync(ISessionStorageService sessionStorageService, ILocalStorageService localStorageService);
    Task LoadStateAsync(ISessionStorageService sessionStorageService, ILocalStorageService localStorageService);
}