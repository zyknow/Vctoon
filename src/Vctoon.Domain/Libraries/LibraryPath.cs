namespace Vctoon.Libraries;

public class LibraryPath : AggregateRoot<Guid>
{
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

    public Library Library { get; set; }

    public LibraryPath? Parent { get; set; }

    public virtual List<LibraryPath> Children { get; } = new();
}