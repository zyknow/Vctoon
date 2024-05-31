using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Identities.Dtos;

[Serializable]
public class IdentityUserLibraryPermissionGrantGetListInput : PagedAndSortedResultRequestDto
{
    [DisplayName("IdentityUserLibraryPermissionGrantUserId")]
    public Guid? UserId { get; set; }
    
    [DisplayName("IdentityUserLibraryPermissionGrantIsAdmin")]
    public bool? IsAdmin { get; set; }
}