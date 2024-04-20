using Vctoon.Libraries;

namespace Vctoon.Comics;

public class Comic : AggregateRoot<Guid>
{
    protected Comic()
    {
    }

    public string Title { get; set; }

    public string CoverPath { get; set; }

    public Guid LibraryId { get; set; }

    public List<Tag> Tags { get; set; } = new();

    public List<ComicChapter> Chapters { get; set; } = new();

    public Comic(
        Guid id,
        string title,
        string coverPath,
        Guid libraryId,
        List<Tag> tags,
        List<ComicChapter> chapters
    ) : base(id)
    {
        Title = title;
        CoverPath = coverPath;
        LibraryId = libraryId;
        Tags = tags;
        Chapters = chapters;
    }
}
