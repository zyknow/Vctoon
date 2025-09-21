namespace Vctoon.Mediums.Dtos.Base;

public abstract class MediumGetListOutputDtoBase : AuditedEntityDto<Guid>, IMediumHasReadingProcessDto
{
    public string Title { get; protected set; }

    public string Description { get; protected set; }

    public string Cover { get; protected set; }

    public int ReadCount { get; protected set; }

    public Guid LibraryId { get; internal set; }
    public double? Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}