using Lucene.Net.Analysis;
using Zyknow.Abp.Lucene.Analyzers;
using Zyknow.Abp.Lucene.Descriptors;
using Zyknow.Abp.Lucene.Fluent;
using LuceneStore = Lucene.Net.Store;

namespace Zyknow.Abp.Lucene.Options;

/// <summary>
/// Lucene 模块的全局配置选项。
/// 控制分析器、索引目录、租户隔离、查询与高亮行为等。
/// </summary>
/// <remarks>
/// - 默认使用 ICU 通用分析器，适合多语言文本；
/// - 索引默认写入本地文件系统目录；
/// - 多租户场景下默认按租户拆分独立索引；
/// - 可通过 <see cref="ConfigureLucene"/> 注册实体搜索描述符。
/// </remarks>
public class LuceneOptions
{
    /// <summary>
    /// 分析器工厂，用于创建 <see cref="Analyzer"/> 实例。
    /// </summary>
    /// <remarks>默认值为 <c>AnalyzerFactories.IcuGeneral</c>。</remarks>
    public Func<Analyzer> AnalyzerFactory { get; set; } = AnalyzerFactories.IcuGeneral;

    /// <summary>
    /// 索引根目录路径（本地文件系统）。
    /// </summary>
    /// <remarks>默认：<c>{AppContext.BaseDirectory}/lucene-index</c>。</remarks>
    public string IndexRootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "lucene-index");

    /// <summary>
    /// 是否按租户隔离索引目录。
    /// </summary>
    /// <remarks>默认启用（true）。</remarks>
    public bool PerTenantIndex { get; set; } = true;

    /// <summary>
    /// 应用启动时是否重建索引。
    /// </summary>
    /// <remarks>默认关闭（false）。</remarks>
    public bool RebuildOnStartup { get; set; } = false;

    /// <summary>
    /// 查询相关配置项（多字段模式、模糊、前缀等）。
    /// </summary>
    public LuceneQueryOptions LuceneQuery { get; } = new();

    /// <summary>
    /// 高亮相关配置项（是否启用、片段大小、片段数）。
    /// </summary>
    public LuceneHighlightOptions LuceneHighlight { get; } = new();

    /// <summary>
    /// 每个实体类型的索引描述符集合。
    /// </summary>
    /// <remarks>通过 <see cref="ConfigureLucene"/> 进行注册。</remarks>
    public Dictionary<Type, EntitySearchDescriptor> Descriptors { get; } = new();

    /// <summary>
    /// 索引目录工厂，创建 <see cref="LuceneStore.Directory"/> 实例。
    /// </summary>
    /// <remarks>
    /// 默认为 <see cref="FSDirectoryFactory"/>（本地文件系统）。
    /// 可替换为 RAM/MMap 或自定义实现以适配不同环境。
    /// </remarks>
    public Func<string, LuceneStore.Directory> DirectoryFactory { get; set; } = FSDirectoryFactory;

    /// <summary>
    /// 是否启用自动事件驱动的索引维护（创建/更新/删除）。
    /// </summary>
    /// <remarks>
    /// 默认启用（true）。关闭后将不再通过实体事件自动维护索引，需手动调用 IndexManager。
    /// </remarks>
    public bool EnableAutoIndexingEvents { get; set; } = true;

    /// <summary>
    /// 使用流式 API 注册实体的索引描述符。
    /// </summary>
    /// <param name="configure">对 <see cref="Fluent.LuceneModelBuilder"/> 的配置委托。</param>
    public void ConfigureLucene(Action<LuceneModelBuilder> configure)
    {
        var builder = new LuceneModelBuilder();
        configure(builder);
        foreach (var kv in builder.Build())
        {
            Descriptors[kv.Key] = kv.Value;
        }
    }

    /// <summary>
    /// 默认文件系统目录工厂：确保目录存在并返回 <see cref="LuceneStore.FSDirectory"/>。
    /// </summary>
    /// <param name="path">索引目录的绝对路径。</param>
    /// <returns>已打开的 <see cref="LuceneStore.Directory"/> 实例。</returns>
    public static LuceneStore.Directory FSDirectoryFactory(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        dirInfo.Create();
        return LuceneStore.FSDirectory.Open(dirInfo);
    }
}