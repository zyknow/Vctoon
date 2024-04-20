using Vctoon.Libraries;

namespace Vctoon.Comics;

public class Comic : AggregateRoot<Guid>
{
    protected Comic()
    {
    }

    public string Title { get; set; }

    public string CoverPath { get; set; }
    
    public Library Library { get; set; }

    public List<Tag> Tags { get; set; } = new();

    public List<ComicChapter> Chapters { get; set; } = new();
}