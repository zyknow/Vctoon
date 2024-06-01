using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Identities.Dtos;

[Serializable]
public class IdentityUserExtraDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
}