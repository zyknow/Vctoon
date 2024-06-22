using Vctoon.Libraries.Dtos;
using Volo.Abp.Identity;

namespace Vctoon.Identities.Dtos;

public class IdentityUserExtraDto : IdentityUserDto
{
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
    public bool IsAdmin { get; set; }
}