namespace Vctoon.Systems;

public interface ISystemAppService : IRemoteService
{
    Task<List<string>> GetSystemPathsAsync(string? path = null);
}