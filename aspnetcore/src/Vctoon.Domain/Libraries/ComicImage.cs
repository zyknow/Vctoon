namespace Vctoon.Libraries;

public class ComicImage : Entity<Guid>
{
    protected ComicImage()
    {
    }

    public ComicImage(
        Guid id,
        string name,
        string path,
        string extension,
        long size,
        Guid comicId,
        Guid libraryId,
        Guid? libraryPathId = null,
        Guid? archiveInfoPathId = null
    ) : base(id)
    {
        Name = name;
        Path = path;
        Extension = extension;
        Size = size;
        LibraryPathId = libraryPathId;
        ArchiveInfoPathId = archiveInfoPathId;
        ComicId = comicId;
        LibraryId = libraryId;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
    public long Size { get; set; }
    public Guid? LibraryPathId { get; set; }
    public Guid? ArchiveInfoPathId { get; set; }

    public Guid LibraryId { get; set; }
    public Guid ComicId { get; set; }
}