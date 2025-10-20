namespace Zyknow.Abp.Lucene.Dtos;

/// <summary>
/// 搜索命中项。
/// - <see cref="EntityId"/>：命中文档的实体主键（默认从描述器的 <c>IdFieldName</c> 字段读取）。
/// - <see cref="Score"/>：Lucene 计算的相关性分值。
/// - <see cref="Payload"/>：参与存储的字段键值对（例如标题、作者等），用于结果展示或后续加载。
/// - <see cref="Highlights"/>：高亮片段（预留，后续版本提供）。
/// </summary>
public class SearchHitDto
{
    /// <summary>
    /// 实体主键（字符串格式）。
    /// 来自索引文档中存储的 id 字段（默认名为 <c>Id</c>，可在实体描述器中调整）。
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// 命中相关性分值，由 Lucene 计算，值越高相关性越强。
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// 高亮片段集合：键为字段名，值为片段列表。
    /// 当前实现中未生成高亮，仅为未来扩展预留；如需启用，需在字段级存储词向量并结合高亮器产出片段。
    /// </summary>
    public Dictionary<string, List<string>> Highlights { get; set; } = new();

    /// <summary>
    /// 存储字段的键值对（例如：Title、Author 等）。
    /// 仅当字段在 Fluent 配置中调用 <c>Store()</c> 时才会回传。
    /// </summary>
    public Dictionary<string, string> Payload { get; set; } = new();
}