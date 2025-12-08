namespace Vctoon.Libraries.Dtos;

[Serializable]
public class ComicImageDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string Path { get; set; }

    public long Size { get; set; }

    public Guid? LibraryPathId { get; set; }

    public Guid? ArchiveInfoPathId { get; set; }

    public Guid MediumId { get; set; }

    public Guid LibraryId { get; set; }
}