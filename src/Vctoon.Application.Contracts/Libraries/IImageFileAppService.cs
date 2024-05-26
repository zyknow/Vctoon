using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface IImageFileAppService : ICrudAppService<ImageFileDto, Guid, ImageFileGetListInput,
    ImageFileCreateUpdateDto, ImageFileCreateUpdateDto>
{
}