using Volo.Abp.Application.Dtos;

namespace Vctoon.Identities.Dtos;

[Serializable]
public class IdentityUserLibraryPermissionGrantDto : AuditedEntityDto<Guid>
{
    public Guid UserId { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
}