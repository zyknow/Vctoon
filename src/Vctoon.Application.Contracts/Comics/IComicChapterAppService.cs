using Vctoon.Comics.Dtos;
using Volo.Abp.Application.Dtos;
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
    Task<PagedResultDto<ComicChapterDto>> GetListAsync(ComicChapterGetListInput input);
    Task UpdateProgressAsync(Guid id, double progress);
    Task<ComicChapterDto> GetAsync(Guid id);
}