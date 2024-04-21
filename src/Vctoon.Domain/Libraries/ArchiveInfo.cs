namespace Vctoon.Libraries;

public class ArchiveInfo : Entity<Guid>
{
    protected ArchiveInfo()
    {
    }

    public ArchiveInfo(
        Guid id,
        string path,
        string extension,
        DateTime? lastModifyTime = null,
        DateTime? lastResolveTime = null
    ) : base(id)
    {
        Path = path;
        Extension = extension;
        LastModifyTime = lastModifyTime;
        LastResolveTime = lastResolveTime;
    }

    public string Path { get; set; }
    public string Extension { get; set; }
    public DateTime? LastModifyTime { get; set; }
    public DateTime? LastResolveTime { get; set; }
    public List<ArchiveInfoPath> Paths { get; set; } = new();
}