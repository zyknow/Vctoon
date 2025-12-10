namespace Vctoon.Mediums.Dtos;

public class MediumGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? LibraryId { get; set; }
    public MediumType? MediumType { get; set; }

    /// <summary>
    /// 是否包含 Series（系列容器）。默认 true。
    /// </summary>
    public bool IncludeSeries { get; set; } = true;

    /// <summary>
    /// 是否包含普通 Medium（非系列）。默认 true。
    /// </summary>
    public bool IncludeMediums { get; set; } = true;

    public bool? HasReadCount { get; set; }

    public int? CreatedInDays { get; set; }

    public List<Guid>? Tags { get; set; }
    public List<Guid>? Artists { get; set; }

    public ReadingProgressType? ReadingProgressType { get; set; }
}
