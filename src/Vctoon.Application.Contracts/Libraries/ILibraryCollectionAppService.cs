using System;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public interface ILibraryCollectionAppService :
    ICrudAppService<
        LibraryCollectionDto,
        Guid,
        LibraryCollectionGetListInput,
        CreateUpdateLibraryCollectionDto,
        CreateUpdateLibraryCollectionDto>
{
}