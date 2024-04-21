namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicChapterCreateUpdateDto
{
    public string Title { get; set; }

    public string CoverPath { get; set; }

    public uint PageCount { get; set; }

    public uint Size { get; set; }

    public Guid ComicId { get; set; }

    // public List<ImageFile> Images { get; set; }
    //
    // public List<Tag> Tags { get; set; }
}