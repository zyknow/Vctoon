namespace Vctoon.Libraries;

public class ArchiveInfo : Entity<Guid>
{
    protected ArchiveInfo()
    {
    }

    public ArchiveInfo(
        Guid id,
        string path,
        string name,
        string extension,
        DateTime? lastModifyTime,
        DateTime? lastResolveTime,
        List<ArchiveInfoPath> paths
    ) : base(id)
    {
        Path = path;
        Name = name;
        Extension = extension;
        LastModifyTime = lastModifyTime;
        LastResolveTime = lastResolveTime;
        Paths = paths;
    }

    public string Path { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public DateTime? LastModifyTime { get; set; }
    public DateTime? LastResolveTime { get; set; }
    public List<ArchiveInfoPath> Paths { get; set; } = new();
}