using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface IImageFileAppService : IApplicationService
{
    Task<PagedResultDto<ImageFileDto>> GetListAsync(ImageFileGetListInput input);

    Task DeleteAsync(Guid id, bool deleteFile);
    Task<List<ImageFileDto>> GetListByComicIdAsync(Guid comicId);
}