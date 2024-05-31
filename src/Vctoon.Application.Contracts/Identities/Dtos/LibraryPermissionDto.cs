using Volo.Abp.Application.Dtos;

namespace Vctoon.Identities.Dtos;

[Serializable]
public class LibraryPermissionDto : AuditedEntityDto<Guid>
{
    public Guid LibraryId { get; set; }
    
    public Guid IdentityUserLibraryPermissionGrantId { get; set; }
    
    public bool CanDownload { get; set; }
    
    public bool CanComment { get; set; }
    
    public bool CanStar { get; set; }
    
    public bool CanView { get; set; }
    
    public bool CanShare { get; set; }
}