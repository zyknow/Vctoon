namespace Vctoon.Libraries;

public class LibraryPermission : AuditedEntity<Guid>
{
    public Guid LibraryId { get; set; }
    public Guid IdentityUserExtraId { get; set; }
    
    public bool CanDownload { get; set; }
    public bool CanComment { get; set; }
    public bool CanStar { get; set; }
    public bool CanView { get; set; }
    public bool CanShare { get; set; }
}