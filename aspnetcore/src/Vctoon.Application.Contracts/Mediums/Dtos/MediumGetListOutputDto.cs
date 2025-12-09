namespace Vctoon.Mediums.Dtos;

public class MediumGetListOutputDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; }

    public string Description { get; set; }

    public MediumType MediumType { get; set; }

    public string Cover { get; set; }

    public int ReadCount { get; set; }

    public Guid LibraryId { get; set; }
    public double? ReadingProgress { get; set; }
    public DateTime? ReadingLastTime { get; set; }
}