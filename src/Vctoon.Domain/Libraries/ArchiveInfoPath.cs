namespace Vctoon.Libraries;

public class ArchiveInfoPath : AggregateRoot<Guid>
{
    public string Path { get; set; }
    public string Name { get; set; }
    public bool IsEmpty { get; set; }
    public DateTime? LastModifyTime { get; set; }
    public DateTime? LastResolveTime { get; set; }
    public List<ComicImageFile> ComicImageFiles { get; set; } = new();
    public List<ArchiveInfoPath> Children { get; set; } = new();
    public Guid? ParentId { get; set; }
    public ArchiveInfoPath? Parent { get; set; }
}