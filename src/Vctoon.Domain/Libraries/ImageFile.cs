namespace Vctoon.Libraries;

public class ImageFile : Entity<Guid>
{
    protected ImageFile()
    {
    }

    public ImageFile(
        Guid id,
        string name,
        string path,
        string extension,
        long size,
        long width,
        long height,
        Guid comicChapterId,
        Guid? libraryPathId = null,
        Guid? archiveInfoPathId = null
    ) : base(id)
    {
        Name = name;
        Path = path;
        Extension = extension;
        Size = size;
        Width = width;
        Height = height;
        LibraryPathId = libraryPathId;
        ArchiveInfoPathId = archiveInfoPathId;
        ComicChapterId = comicChapterId;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
    public long Size { get; set; }
    public long Width { get; set; }
    public long Height { get; set; }


    public Guid? LibraryPathId { get; set; }
    public Guid? ArchiveInfoPathId { get; set; }
    public Guid ComicChapterId { get; set; }

    public List<Tag> Tags { get; set; } = new();
}