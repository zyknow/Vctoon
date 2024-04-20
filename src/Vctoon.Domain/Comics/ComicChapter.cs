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
        uint pageCount,
        uint size,
        Guid comicId,
        List<ImageFile> images,
        List<Tag> tags
    ) : base(id)
    {
        Title = title;
        CoverPath = coverPath;
        PageCount = pageCount;
        Size = size;
        ComicId = comicId;
        Images = images;
        Tags = tags;
    }

    public string Title { get; set; }
    public string CoverPath { get; set; }

    public uint PageCount { get; set; }

    public uint Size { get; set; }

    public Guid ComicId { get; set; }

    public List<ImageFile> Images { get; set; } = new();

    public List<Tag> Tags { get; set; } = new();
}