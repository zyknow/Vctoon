using Vctoon.Comics.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Comics;

public interface IComicChapterAppService :
    ICrudAppService<
        ComicChapterDto,
        Guid,
        ComicChapterGetListInput,
        ComicChapterCreateUpdateDto,
        ComicChapterCreateUpdateDto>
{
}