using System.IO;
using Vctoon.Mediums.Dtos;

namespace Vctoon.Mediums;

public interface IMediumResourceAppService : IApplicationService
{
    Task<RemoteStreamContent> GetCoverAsync(string cover);
    Task UpdateReadingProcessAsync(List<ReadingProcessUpdateDto> items);
    Task AddArtistListAsync(MediumMultiUpdateDto input);
    Task UpdateArtistListAsync(MediumMultiUpdateDto input);
    Task DeleteArtistListAsync(MediumMultiUpdateDto input);
    Task UpdateTagListAsync(MediumMultiUpdateDto input);
    Task AddTagListAsync(MediumMultiUpdateDto input);
    Task DeleteTagListAsync(MediumMultiUpdateDto input);
}