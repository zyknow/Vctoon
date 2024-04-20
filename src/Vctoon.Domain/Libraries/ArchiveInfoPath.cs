namespace Vctoon.Libraries;

public class ArchiveInfoPath : Entity<Guid>
{
    protected ArchiveInfoPath()
    {
    }

    public ArchiveInfoPath(
        Guid id,
        string path,
        string name,
        bool isEmpty,
        Guid archiveInfoId,
        Guid? parentId = null
    ) : base(id)
    {
        Path = path;
        Name = name;
        IsEmpty = isEmpty;
        ArchiveInfoId = archiveInfoId;
        ParentId = parentId;
    }

    public string Path { get; set; }
    public string Name { get; set; }
    public bool IsEmpty { get; set; }

    public Guid ArchiveInfoId { get; set; }

    public Guid? ParentId { get; set; }

    public List<ArchiveInfoPath> Children { get; set; } = new();
}