namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryPermissionDto : EntityDto
{
    public Guid LibraryId { get; set; }

    public Guid UserId { get; set; }

    public bool CanDownload { get; set; }

    public bool CanComment { get; set; }

    public bool CanStar { get; set; }

    public bool CanView { get; set; }

    public bool CanShare { get; set; }
}