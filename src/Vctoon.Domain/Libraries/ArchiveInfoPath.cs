namespace Vctoon.Libraries;

public class ArchiveInfoPath : Entity<Guid>
{
    protected ArchiveInfoPath()
    {
    }

    public ArchiveInfoPath(
        Guid id,
        string? path,
        string name,
        Guid archiveInfoId,
        DateTime? lastModifyTime = null,
        DateTime? lastResolveTime = null
    ) : base(id)
    {
        Path = path;
        Name = name;
        ArchiveInfoId = archiveInfoId;
        LastModifyTime = lastModifyTime;
        LastResolveTime = lastResolveTime;
    }

    public string? Path { get; set; }
    public string Name { get; set; }

    public DateTime? LastModifyTime { get; set; }
    public DateTime? LastResolveTime { get; set; }

    public Guid ArchiveInfoId { get; set; }
}