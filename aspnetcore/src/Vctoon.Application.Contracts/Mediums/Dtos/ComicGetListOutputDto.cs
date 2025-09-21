namespace Vctoon.Mediums.Dtos;

[Serializable]
public class ComicGetListOutputDto : MediumGetListOutputDtoBase, IMediumHasReadingProcessDto
{
    public double? Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}