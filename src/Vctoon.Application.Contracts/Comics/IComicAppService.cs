using System;
using Vctoon.Comics.Dtos;
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

}