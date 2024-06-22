using Vctoon.Libraries.Dtos;
using Volo.Abp.Identity;

namespace Vctoon.Identities.Dtos;

public class IdentityUserExtraCreateDto : IdentityUserCreateDto
{
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
}