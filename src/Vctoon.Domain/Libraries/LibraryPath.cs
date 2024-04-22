namespace Vctoon.Libraries;

public class LibraryPath : Entity<Guid>
{
    protected LibraryPath()
    {
    }

    public LibraryPath(
        Guid id,
        string path,
        bool isRoot,
        Guid libraryId,
        DateTime? lastResolveTime = null,
        DateTime? lastModifyTime = null
    ) : base(id)
    {
        SetPath(path);
        IsRoot = isRoot;
        LastResolveTime = lastResolveTime;
        LastModifyTime = lastModifyTime;
        LibraryId = libraryId;
    }

    /// <summary>
    /// dirPath or archivePath
    /// </summary>
    public string Path { get; protected set; }

    public bool IsRoot { get; set; }

    /// <summary>
    /// use directory LastWriteTimeUtc
    /// </summary>
    public DateTime? LastModifyTime { get; set; }

    public DateTime? LastResolveTime { get; set; }

    public Guid LibraryId { get; set; }

    public void SetPath(string path)
    {
        if (path.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(path));
        }

        Path = path;
    }
}