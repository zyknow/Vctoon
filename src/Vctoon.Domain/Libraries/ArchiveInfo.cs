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
        Guid libraryPathId,
        DateTime? lastModifyTime = null,
        DateTime? lastResolveTime = null
    ) : base(id)
    {
        SetPath(path);
        SetExtension(extension);
        LastModifyTime = lastModifyTime;
        LastResolveTime = lastResolveTime;
        LibraryPathId = libraryPathId;
    }
    
    public string Path { get; protected set; }
    public string Extension { get; protected set; }
    public DateTime? LastModifyTime { get; set; }
    public DateTime? LastResolveTime { get; set; }
    public List<ArchiveInfoPath> Paths { get; } = new();
    
    public Guid LibraryPathId { get; set; }
    
    public void SetPath(string path)
    {
        Check.NotNullOrWhiteSpace(path, nameof(path));
        Path = path;
    }
    
    public void SetExtension(string extension)
    {
        Check.NotNullOrWhiteSpace(extension, nameof(extension));
        Extension = extension;
    }
}