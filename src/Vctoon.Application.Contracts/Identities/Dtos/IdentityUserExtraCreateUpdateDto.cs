using Vctoon.Libraries.Dtos;

namespace Vctoon.Identities.Dtos;

[Serializable]
public class IdentityUserExtraCreateUpdateDto
{
    public Guid UserId { get; set; }
    
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
}