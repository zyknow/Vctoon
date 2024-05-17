using Vctoon.Comics.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace Vctoon.Comics;

public interface IComicAppService :
    ICrudAppService<
        ComicDto,
        Guid,
        ComicGetListInput,
        ComicCreateUpdateDto,
        ComicCreateUpdateDto>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="toCompleted"></param>
    /// <exception cref="UserFriendlyException"></exception>
    Task ChangeProcessStatusAsync(Guid id, bool toCompleted);
    
    Task<List<ComicChapterDto>> GetAllChaptersAsync(Guid comicId);
}