namespace Vctoon.Mediums.Dtos;

[Serializable]
public class ComicDto : MediumDtoBase, IMediumHasReadingProcessDto
{
    public List<ComicImageDto> ComicImages { get; set; }
    public double? ReadingProgress { get; set; }
    public DateTime? ReadingLastTime { get; set; }
}