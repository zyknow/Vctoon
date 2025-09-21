using System.IO;
using Vctoon.Mediums.Dtos;

namespace Vctoon.Mediums;

public interface IMediumResourceAppService : IApplicationService
{
    Task<Stream> GetCoverAsync(string cover);
    Task UpdateReadingProcessAsync(ReadingProcessUpdateDto input);
}