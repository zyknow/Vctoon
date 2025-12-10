using Vctoon.Mediums.Dtos;

namespace Vctoon.Mediums;

public interface IMediumAppService : ICrudAppService<
    MediumDto,
    Guid,
    MediumGetListInput,
    CreateUpdateMediumDto>
{
    Task<List<MediumDto>> GetSeriesListAsync(Guid mediumId);
    Task UpdateSeriesSortAsync(MediumSeriesSortUpdateDto input);

    Task AddArtistListAsync(MediumMultiUpdateDto input);
    Task AddTagListAsync(MediumMultiUpdateDto input);
    Task DeleteArtistListAsync(MediumMultiUpdateDto input);
    Task DeleteComicImageAsync(Guid comicImageId, bool deleteFile);
    Task DeleteTagListAsync(MediumMultiUpdateDto input);
    Task<RemoteStreamContent> GetComicImageAsync(Guid comicImageId, int? maxWidth = null);
    Task<List<ComicImageDto>> GetComicImageListAsync(Guid mediumId);
    Task<RemoteStreamContent> GetCoverAsync(string cover);
    Task<IRemoteStreamContent> GetSubtitleAsync(Guid id, string file);
    Task<List<SubtitleDto>> GetSubtitlesAsync(Guid id);
    Task UpdateArtistListAsync(MediumMultiUpdateDto input);
    Task<MediumDto> UpdateArtistsAsync(Guid id, List<Guid> artistIds);
    Task<MediumDto> UpdateCoverAsync(Guid id, RemoteStreamContent cover);
    Task UpdateReadingProcessAsync(List<ReadingProcessUpdateDto> items);
    Task UpdateTagListAsync(MediumMultiUpdateDto input);
    Task<MediumDto> UpdateTagsAsync(Guid id, List<Guid> tagIds);
}
