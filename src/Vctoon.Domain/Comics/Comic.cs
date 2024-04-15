using Vctoon.Libraries;

namespace Vctoon.Comics;

public class Comic : AggregateRoot<Guid>
{
    protected Comic()
    {
    }

    public Comic(
        Guid id,
        Guid libraryId,
        string title,
        string coverPath
    ) : base(id)
    {
        LibraryId = libraryId;
        Title = title;
        CoverPath = coverPath;
    }

    public string Title { get; set; }

    public string CoverPath { get; set; }

    public Guid LibraryId { get; set; }

    public Library Library { get; set; }

    public List<ComicChapter> Chapters { get; set; } = new();
}