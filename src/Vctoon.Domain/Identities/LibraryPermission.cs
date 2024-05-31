namespace Vctoon.Identities;

public class LibraryPermission : AuditedEntity<Guid>
{
    public Guid LibraryId { get; set; }
    public Guid IdentityUserLibraryPermissionGrantId { get; set; }
    
    public bool CanDownload { get; set; }
    public bool CanComment { get; set; }
    public bool CanStar { get; set; }
    public bool CanView { get; set; }
    public bool CanShare { get; set; }
}