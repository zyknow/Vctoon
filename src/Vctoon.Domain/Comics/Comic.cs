using Vctoon.Libraries;

namespace Vctoon.Comics;

public class Comic : AggregateRoot<Guid>
{
    protected Comic()
    {
    }

    public Comic(
        Guid id,
        string title,
        string coverPath,
        Guid libraryId
    ) : base(id)
    {
        Title = title;
        CoverPath = coverPath;
        LibraryId = libraryId;
    }

    public string Title { get; set; }

    public string CoverPath { get; set; }

    public Guid LibraryId { get; set; }

    public List<Tag> Tags { get; set; } = new();

    public List<ComicChapter> Chapters { get; set; } = new();
}