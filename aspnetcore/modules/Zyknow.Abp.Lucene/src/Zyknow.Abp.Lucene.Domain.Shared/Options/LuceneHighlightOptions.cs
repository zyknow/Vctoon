namespace Zyknow.Abp.Lucene.Options;

/// <summary>
/// 高亮选项：控制是否启用高亮及片段相关设置。
/// </summary>
public class LuceneHighlightOptions
{
    /// <summary>
    /// 是否启用高亮。
    /// </summary>
    /// <remarks>默认 false。开启后返回命中文本片段。</remarks>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 高亮片段的字符长度。
    /// </summary>
    /// <remarks>默认 150。值越大单个片段包含更多上下文。</remarks>
    public int FragmentSize { get; set; } = 150;

    /// <summary>
    /// 每条命中返回的高亮片段最大数量。
    /// </summary>
    /// <remarks>默认 2。可根据 UI 展示需求调整。</remarks>
    public int MaxFragments { get; set; } = 2;
}