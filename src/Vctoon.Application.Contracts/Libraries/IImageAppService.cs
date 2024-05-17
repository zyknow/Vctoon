using Volo.Abp;

namespace Vctoon.Libraries;

public interface IImageAppService : IRemoteService
{
    Task<Stream> GetCoverAsync(string coverPath);
    Task<Stream> GetImageAsync(Guid imageFileId, int? maxWidth = null);
}