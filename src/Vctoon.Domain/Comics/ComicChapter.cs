using Vctoon.Libraries;

namespace Vctoon.Comics;

public class ComicChapter : Entity<Guid>
{
    protected ComicChapter()
    {
    }

    public ComicChapter(
        Guid id,
        string coverPath,
        string title,
        Guid comicId,
        Comic comic,
        uint pageCount,
        uint size,
        Guid libraryPathId
    ) : base(id)
    {
        CoverPath = coverPath;
        Title = title;
        ComicId = comicId;
        Comic = comic;
        PageCount = pageCount;
        Size = size;
        LibraryPathId = libraryPathId;
    }

    public string CoverPath { get; set; }

    public string Title { get; set; }


    public Guid ComicId { get; set; }

    public Comic Comic { get; set; }

    public uint PageCount { get; set; }

    public uint Size { get; set; }

    public Guid LibraryPathId { get; set; }
    public LibraryPath LibraryPath { get; set; }
}