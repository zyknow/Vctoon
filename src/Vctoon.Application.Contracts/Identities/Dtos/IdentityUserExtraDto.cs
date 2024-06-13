using Vctoon.Libraries.Dtos;
using Volo.Abp.Identity;

namespace Vctoon.Identities.Dtos;

public class IdentityUserExtraDto : IdentityUserDto
{
    public bool IsAdmin { get; set; }
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
}