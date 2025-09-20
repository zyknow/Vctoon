namespace Vctoon.Mediums.Dtos;

[Serializable]
public class ComicDto : MediumDtoBase
{
    public List<ComicImageDto> ComicImages { get; set; }
}