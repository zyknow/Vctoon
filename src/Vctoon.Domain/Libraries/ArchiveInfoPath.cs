namespace Vctoon.Libraries;

public class ArchiveInfoPath : AggregateRoot<Guid>
{
    public string Path { get; set; }
    public string Name { get; set; }
    public bool IsEmpty { get; set; }
    
    public ArchiveInfoPath? Parent { get; set; }
    
    public List<ArchiveInfoPath> Children { get; set; } = new();
}