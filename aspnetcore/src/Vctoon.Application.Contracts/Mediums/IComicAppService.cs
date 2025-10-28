using Vctoon.Mediums.Dtos;

namespace Vctoon.Mediums;

public interface IComicAppService : IMediumBaseAppService<ComicDto, ComicGetListOutputDto, ComicGetListInput,
    ComicCreateUpdateDto, ComicCreateUpdateDto>
{
    Task<RemoteStreamContent> GetComicImageAsync(Guid comicImageId, int? maxWidth = null);
    Task<List<ComicImageDto>> GetListByComicIdAsync(Guid comicId);
    Task DeleteComicImageAsync(Guid comicImageId, bool deleteFile);
}