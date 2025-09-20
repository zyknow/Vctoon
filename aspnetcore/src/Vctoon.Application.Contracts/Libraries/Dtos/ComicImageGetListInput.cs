namespace Vctoon.Libraries.Dtos;

[Serializable]
public class ComicImageGetListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }

    public string? Path { get; set; }

    public string? Extension { get; set; }

    public long? Size { get; set; }

    public Guid? LibraryPathId { get; set; }

    public Guid? ArchiveInfoPathId { get; set; }

    public Guid? LibraryId { get; set; }

    public Guid? MediumId { get; set; }
}