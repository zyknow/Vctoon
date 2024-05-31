using System.ComponentModel;

namespace Vctoon.Identities.Dtos;

[Serializable]
public class IdentityUserLibraryPermissionGrantCreateUpdateDto
{
    [DisplayName("IdentityUserLibraryPermissionGrantUserId")]
    public Guid UserId { get; set; }
    
    [DisplayName("IdentityUserLibraryPermissionGrantIsAdmin")]
    public bool IsAdmin { get; set; }
    
    [DisplayName("IdentityUserLibraryPermissionGrantLibraryPermissions")]
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
}