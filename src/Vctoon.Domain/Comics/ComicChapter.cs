using Vctoon.Libraries;

namespace Vctoon.Comics;

public class ComicChapter : Entity<Guid>
{
    public string Title { get; set; }
    public string CoverPath { get; set; }

    public uint PageCount { get; set; }
    
    public uint Size { get; set; }
    
    public List<ImageFile> Images { get; set; } = new();

    public List<Tag> Tags { get; set; } = new();
}