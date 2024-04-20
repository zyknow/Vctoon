namespace Vctoon.Libraries;

public class ImageFile : AggregateRoot<Guid>
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
    public uint Size { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    public LibraryPath? LibraryPath { get; set; }
    public ArchiveInfoPath? ArchiveInfoPath { get; set; }

    public List<Tag> Tags { get; set; } = new ();
}