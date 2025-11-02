namespace Vctoon.Mediums.Dtos.Base;

public abstract class MediumGetListInputBase : PagedAndSortedResultRequestDto, IMediumHasReadingProcessQuery
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool? HasReadCount { get; set; }

    public int? CreatedInDays { get; set; }

    public List<Guid>? Tags { get; set; }
    public List<Guid>? Artists { get; set; }

    public Guid? LibraryId { get; set; }

    public ReadingProgressType? ReadingProgressType { get; set; }
}