namespace Vctoon.Mediums.Dtos;

[Serializable]
public class ComicDto : MediumDtoBase, IMediumHasReadingProcessDto
{
    public List<ComicImageDto> ComicImages { get; set; }
    public double? Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}