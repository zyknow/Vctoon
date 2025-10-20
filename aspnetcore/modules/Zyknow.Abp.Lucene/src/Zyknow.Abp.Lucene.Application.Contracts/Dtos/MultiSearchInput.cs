namespace Zyknow.Abp.Lucene.Dtos;

/// <summary>
/// 多实体搜索输入。
/// </summary>
public class MultiSearchInput : SearchQueryInput
{
    /// <summary>
    /// 参与聚合的实体索引名集合。
    /// </summary>
    public List<string> Entities { get; set; } = new();

    /// <summary>
    /// 原始查询字符串。支持 Lucene 的解析器语法（例如：
    /// <code>title:Lucene AND author:Erik</code>、短语查询 <code>"Pro .NET"</code>、前缀 <code>Luc*</code> 等）。
    /// 若未指定字段，默认使用实体在 Fluent 配置中声明的所有参与索引的字段。
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用模糊查询（Fuzzy）。
    /// - 模糊匹配将依据全局选项 <c>FuzzyMaxEdits</c>（Levenshtein 编辑距离）进行召回。
    /// - 注意：模糊查询会降低精确度并可能影响性能，请按需使用。
    /// </summary>
    public bool Fuzzy { get; set; } = false;

    /// <summary>
    /// 是否启用前缀匹配（Prefix）。
    /// - 适用于前缀词条召回，若在索引侧启用了 <c>Autocomplete</c>（NGram/EdgeNGram），可显著提升效果。
    /// - 未启用自动补全索引时，前缀匹配仅对已分词生成的前缀有效，召回可能受限。
    /// </summary>
    public bool Prefix { get; set; } = true;

    /// <summary>
    /// 是否请求高亮片段（Highlights）。
    /// - 当前实现中，仅返回 <c>Payload</c>（参与存储的字段值）；高亮片段功能预留，后续版本将结合词向量/高亮器提供。
    /// - 若未来开启高亮，需要在字段级启用 <c>StoreTermVectors</c> 并选择合适的高亮策略。
    /// </summary>
    public bool Highlight { get; set; } = false;
}