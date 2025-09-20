using System.IO;

namespace Vctoon.Libraries;

public interface IResourceAppService : IRemoteService
{
    Task<Stream> GetCoverAsync(string coverPath);
    Task<Stream> GetImageAsync(Guid imageFileId, int? maxWidth = null);
}