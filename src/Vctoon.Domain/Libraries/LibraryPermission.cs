namespace Vctoon.Libraries;

public class LibraryPermission : IEntity
{
    protected LibraryPermission()
    {
    }

    public LibraryPermission(
        Guid libraryId,
        Guid userId,
        bool canDownload,
        bool canComment,
        bool canStar,
        bool canView,
        bool canShare)
    {
        LibraryId = libraryId;
        UserId = userId;
        CanDownload = canDownload;
        CanComment = canComment;
        CanStar = canStar;
        CanView = canView;
        CanShare = canShare;
    }

    public Guid LibraryId { get; set; }
    public Guid UserId { get; set; }
    public bool CanDownload { get; set; }
    public bool CanComment { get; set; }
    public bool CanStar { get; set; }
    public bool CanView { get; set; }
    public bool CanShare { get; set; }

    public object?[] GetKeys()
    {
        return new object?[] { LibraryId, UserId };
    }
}