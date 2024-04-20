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
        uint size,
        uint width,
        uint height,
        Guid? libraryPathId,
        Guid? archiveInfoPathId,
        Guid comicChapterId,
        List<Tag> tags
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
        Tags = tags;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
    public uint Size { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }


    public Guid? LibraryPathId { get; set; }
    public Guid? ArchiveInfoPathId { get; set; }
    public Guid ComicChapterId { get; set; }

    public List<Tag> Tags { get; set; } = new();
}