namespace Vctoon.Libraries;

public class ArchiveInfo : AggregateRoot<Guid>
{
    public string Path { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public DateTime? LastModifyTime { get; set; }
    public DateTime? LastResolveTime { get; set; }
    public List<ArchiveInfoPath> Paths { get; set; } = new();
}