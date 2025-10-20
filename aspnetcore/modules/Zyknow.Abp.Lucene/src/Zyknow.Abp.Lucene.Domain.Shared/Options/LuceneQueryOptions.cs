namespace Zyknow.Abp.Lucene.Options;

/// <summary>
/// 查询选项：控制多字段模式、模糊匹配和前缀匹配。
/// </summary>
public class LuceneQueryOptions
{
    /// <summary>
    /// 多字段查询模式，支持 "OR" 或 "AND"。
    /// </summary>
    /// <remarks>"OR" 召回更宽松，"AND" 更严格。默认 "OR"。</remarks>
    public string MultiFieldMode { get; set; } = "OR"; // OR or AND

    /// <summary>
    /// 是否启用模糊查询（编辑距离）。
    /// </summary>
    /// <remarks>默认 false。开启后使用 Levenshtein 距离进行近似匹配。</remarks>
    public bool FuzzyEnabled { get; set; } = false;

    /// <summary>
    /// 模糊查询的最大编辑距离（Levenshtein）。
    /// </summary>
    /// <remarks>默认 1。值越大召回更多、精准度可能下降。</remarks>
    public int FuzzyMaxEdits { get; set; } = 1; // Levenshtein

    /// <summary>
    /// 是否启用前缀匹配查询。
    /// </summary>
    /// <remarks>默认 true。适用于自动补全等场景。</remarks>
    public bool PrefixEnabled { get; set; } = true;
}