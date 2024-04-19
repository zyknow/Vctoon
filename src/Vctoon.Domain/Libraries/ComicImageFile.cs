using Vctoon.Comics;

namespace Vctoon.Libraries;

public class ComicImageFile : AggregateRoot<Guid>
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
    public uint Size { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
    
    public Guid? ArchiveInfoPathId { get; set; }
    public ArchiveInfoPath? ArchiveInfoPath { get; set; }
    
    public Guid? ComicChapterId { get; set; }
    public ComicChapter? ComicChapter { get; set; }
}