using Vctoon.Mediums.Dtos;

namespace Vctoon.Mediums;

public interface IComicAppService : IMediumBaseAppService<ComicDto, ComicGetListOutputDto, ComicGetListInput,
    ComicCreateUpdateDto, ComicCreateUpdateDto>
{
}