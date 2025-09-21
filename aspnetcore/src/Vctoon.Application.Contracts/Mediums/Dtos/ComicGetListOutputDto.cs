namespace Vctoon.Mediums.Dtos;

[Serializable]
public class ComicGetListOutputDto : MediumGetListOutputDtoBase, IMediumHasReadingProcessDto
{
    public double? ReadingProgress { get; set; }
    public DateTime? ReadingLastTime { get; set; }
}