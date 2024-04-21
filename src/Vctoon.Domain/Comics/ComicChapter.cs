using Vctoon.Libraries;

namespace Vctoon.Comics;

public class ComicChapter : Entity<Guid>
{
    protected ComicChapter()
    {
    }

    public ComicChapter(
        Guid id,
        string title,
        string coverPath,
        long pageCount,
        long size,
        Guid comicId
    ) : base(id)
    {
        Title = title;
        CoverPath = coverPath;
        PageCount = pageCount;
        Size = size;
        ComicId = comicId;
    }

    public string Title { get; set; }
    public string CoverPath { get; set; }

    public long PageCount { get; set; }

    public long Size { get; set; }

    public Guid ComicId { get; set; }

    public List<ImageFile> Images { get; set; } = new();

    public List<Tag> Tags { get; set; } = new();
}