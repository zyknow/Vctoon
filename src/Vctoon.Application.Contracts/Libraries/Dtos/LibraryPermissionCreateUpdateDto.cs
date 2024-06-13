using System.ComponentModel;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryPermissionCreateUpdateDto
{
    [DisplayName("LibraryPermissionLibraryId")]
    public Guid LibraryId { get; set; }

    [DisplayName("LibraryPermissionCanDownload")]
    public bool CanDownload { get; set; }

    [DisplayName("LibraryPermissionCanComment")]
    public bool CanComment { get; set; }

    [DisplayName("LibraryPermissionCanStar")]
    public bool CanStar { get; set; }

    [DisplayName("LibraryPermissionCanView")]
    public bool CanView { get; set; }

    [DisplayName("LibraryPermissionCanShare")]
    public bool CanShare { get; set; }
}