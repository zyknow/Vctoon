namespace Vctoon.Mediums.Dtos;

public class MediumGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? LibraryId { get; set; }
    public MediumType? MediumType { get; set; }

    public bool? HasReadCount { get; set; }

    public int? CreatedInDays { get; set; }

    public List<Guid>? Tags { get; set; }
    public List<Guid>? Artists { get; set; }

    public ReadingProgressType? ReadingProgressType { get; set; }
}
