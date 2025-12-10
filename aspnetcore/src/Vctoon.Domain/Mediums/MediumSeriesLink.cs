namespace Vctoon.Mediums;

public class MediumSeriesLink : AuditedEntity<Guid>
{
    protected MediumSeriesLink()
    {
    }

    public MediumSeriesLink(Guid id, Guid seriesId, Guid mediumId, int sort = 0) : base(id)
    {
        SeriesId = seriesId;
        MediumId = mediumId;
        Sort = sort;
    }

    /// <summary>
    /// 系列（也是一个 Medium）。
    /// </summary>
    public Guid SeriesId { get; set; }

    public Medium Series { get; set; } = default!;

    /// <summary>
    /// 条目/分集（也是一个 Medium）。
    /// </summary>
    public Guid MediumId { get; set; }

    public Medium Medium { get; set; } = default!;

    /// <summary>
    /// 系列内排序（可选）。
    /// </summary>
    public int Sort { get; set; }
}
