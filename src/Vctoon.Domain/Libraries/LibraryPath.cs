namespace Vctoon.Libraries;

public class LibraryPath : Entity<Guid>
{
    protected LibraryPath()
    {
    }

    public LibraryPath(
        Guid id,
        string path,
        bool isEmpty,
        DateTime? lastModifyTime,
        DateTime? lastResolveTime,
        Guid libraryId,
        Guid? parentId,
        List<LibraryPath> children
    ) : base(id)
    {
        Path = path;
        IsEmpty = isEmpty;
        LastModifyTime = lastModifyTime;
        LastResolveTime = lastResolveTime;
        LibraryId = libraryId;
        ParentId = parentId;
        Children = children;
    }

    /// <summary>
    /// dirPath or archivePath
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsEmpty { get; set; }

    /// <summary>
    /// use directory LastWriteTimeUtc
    /// </summary>
    public DateTime? LastModifyTime { get; set; }

    public DateTime? LastResolveTime { get; set; }

    public Guid LibraryId { get; set; }

    public Guid? ParentId { get; set; }

    public virtual List<LibraryPath> Children { get; } = new();
}