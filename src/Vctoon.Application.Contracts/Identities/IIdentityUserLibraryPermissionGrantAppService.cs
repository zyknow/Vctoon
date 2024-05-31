using Vctoon.Identities.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Identities;

public interface IIdentityUserLibraryPermissionGrantAppService :
    ICrudAppService<
        IdentityUserLibraryPermissionGrantDto,
        Guid,
        IdentityUserLibraryPermissionGrantGetListInput,
        IdentityUserLibraryPermissionGrantCreateUpdateDto,
        IdentityUserLibraryPermissionGrantCreateUpdateDto>
{
}