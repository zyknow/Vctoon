namespace Vctoon.Libraries;

public class LibraryPermission : AuditedEntity<Guid>
{
    protected LibraryPermission()
    {
    }
    
    public LibraryPermission(
        Guid id,
        Guid libraryId,
        Guid identityUserExtraId,
        bool canDownload,
        bool canComment,
        bool canStar,
        bool canView,
        bool canShare
    ) : base(id)
    {
        LibraryId = libraryId;
        IdentityUserExtraId = identityUserExtraId;
        CanDownload = canDownload;
        CanComment = canComment;
        CanStar = canStar;
        CanView = canView;
        CanShare = canShare;
    }
    
    public Guid LibraryId { get; set; }
    public Guid IdentityUserExtraId { get; set; }
    
    public bool CanDownload { get; set; }
    public bool CanComment { get; set; }
    public bool CanStar { get; set; }
    public bool CanView { get; set; }
    public bool CanShare { get; set; }
}