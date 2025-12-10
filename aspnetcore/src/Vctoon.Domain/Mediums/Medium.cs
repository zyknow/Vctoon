using Vctoon.Identities;

namespace Vctoon.Mediums;

public class Medium : AuditedAggregateRoot<Guid>
{
    protected Medium()
    {
    }

    public Medium(
        Guid id,
        MediumType mediumType,
        string title,
        string cover,
        Guid libraryId,
        Guid? libraryPathId = null,
        string description = ""
    ) : base(id)
    {
        MediumType = mediumType;
        Title = title;
        Cover = cover;
        LibraryId = libraryId;
        LibraryPathId = libraryPathId;
        Description = description;
    }

    public string Title { get; set; }

    public MediumType MediumType { get; set; }

    public string Description { get; set; }

    public string Cover { get; set; }

    /// <summary>
    /// 是否作为“Series（系列）”容器显示。
    /// </summary>
    public bool IsSeries { get; set; }

    public int ReadCount { get; set; }

    public Guid LibraryId { get; set; }

    public Guid? LibraryPathId { get; set; }

    public VideoDetail? VideoDetail { get; set; }

    /// <summary>
    /// 我作为“条目/分集”所归属的系列关联。
    /// </summary>
    public virtual ICollection<MediumSeriesLink> SeriesLinks { get; set; } = new List<MediumSeriesLink>();

    /// <summary>
    /// 我作为“系列”所包含的条目关联。
    /// </summary>
    public virtual ICollection<MediumSeriesLink> ItemLinks { get; set; } = new List<MediumSeriesLink>();

    public virtual List<Tag> Tags { get; } = new();

    public virtual List<Artist> Artists { get; set; } = new();

    public ICollection<IdentityUserReadingProcess> Processes { get; set; } = new List<IdentityUserReadingProcess>();
}