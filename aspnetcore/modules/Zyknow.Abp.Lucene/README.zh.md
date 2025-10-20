# Zyknow.Abp.Lucene

一个基于 Lucene.NET 的 ABP 模块，用于为系统提供可配置的全文搜索能力。

## 特性

- 统一 ICU 通用分析器（多语言友好）
- Fluent API：选择参与索引的字段、关键字字段、权重、自动补全
- 支持多租户索引隔离（可选）
- 索引管理：新增/更新/删除/重建
- 搜索服务：多字段查询、分页、返回存储字段（Payload）

## 安装与集成

- 将 `Zyknow.Abp.Lucene.*` 项目引用到宿主应用
- 在宿主模块 `DependsOn` 中加入本模块

## 快速上手

```csharp
Configure<ZyknowLuceneOptions>(opt =>
{
    opt.IndexRootPath = Path.Combine(AppContext.BaseDirectory, "lucene-index");
    opt.PerTenantIndex = true;
    opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
    opt.ConfigureLucene(model =>
    {
        model.Entity<Book>(e =>
        {
            e.Field(x => x.Title, f => f.Store());
            e.Field(x => x.Author, f => f.Store());
            e.Field(x => x.Code, f => f.Keyword());
        });
    });
});
```

## 索引操作

```csharp
await _indexManager.IndexAsync(book);
await _indexManager.IndexRangeAsync(books, replace: true);
await _indexManager.DeleteAsync<Book>(bookId);
await _indexManager.RebuildAsync(typeof(Book));
```

## 事件自动索引接入

- 开启自动接入：默认启用，可通过 `ZyknowLuceneOptions.EnableAutoIndexingEvents = true/false` 控制。
- 原理：订阅 ABP 本地实体事件（创建/更新/删除），在当前 UoW 事务提交后统一批量维护索引（聚合 Upsert/ Delete，减少写入次数）。
- 仅处理已注册实体：仅对通过 Fluent DSL 注册到 `Descriptors` 的类型生效，未注册的实体事件将被忽略。
- 批量删除优化：使用 `DeleteRangeAsync<T>(IEnumerable<object> ids)` 提升删除性能。
- 多租户支持：在当前租户上下文内执行，索引目录按租户自动隔离。

## 搜索

支持通过 HTTP GET 进行搜索，包含单实体与多实体聚合两类端点。

- 单实体搜索：`GET /api/lucene/search/{entity}`

  - Query 参数（来自 `SearchQueryInput`）：`query`、`fuzzy`、`prefix`、`highlight`、`skipCount`、`maxResultCount`、`sorting`
  - 示例：
    ```bash
    curl -G "http://localhost:5000/api/lucene/search/Book" \
      --data-urlencode "query=Lucene" \
      --data-urlencode "fuzzy=true" \
      --data-urlencode "prefix=true" \
      --data-urlencode "skipCount=0" \
      --data-urlencode "maxResultCount=10"
    ```

- 多实体聚合搜索：`GET /api/lucene/search-many`
  - Query 参数（来自 `MultiSearchInput`）：`entities`（必填，数组），以及 `query`、`fuzzy`、`prefix`、`highlight`、`skipCount`、`maxResultCount`、`sorting`
  - 传递数组参数的方式：`entities=Book&entities=Item`（可重复键）
  - 示例：
    ```bash
    curl -G "http://localhost:5000/api/lucene/search-many" \
      --data-urlencode "entities=Book" \
      --data-urlencode "entities=Item" \
      --data-urlencode "query=title:Lucene OR name:Lucene" \
      --data-urlencode "prefix=true" \
      --data-urlencode "fuzzy=false" \
      --data-urlencode "skipCount=0" \
      --data-urlencode "maxResultCount=20"
    ```

返回值

- 两个端点均返回 `SearchResultDto`（分页）：`TotalCount` 与 `Items`。
- 命中项 `SearchHitDto` 包含：
  - `EntityId`：文档主键（来自各实体描述器的 `IdFieldName`）
  - `Score`：相关性分值
  - `Payload`：存储字段键值对。多实体聚合下还包含 `Payload["__IndexName"]` 用于标识文档来源实体索引名。

注意

- 全局查询行为受 `LuceneOptions.LuceneQuery` 影响：
  - `MultiFieldMode`：`AND`/`OR`（默认为 `OR`）。
  - `FuzzyMaxEdits`：模糊查询编辑距离（默认 1）。
- 若启用了多租户（`PerTenantIndex`），请求在当前租户上下文内检索对应租户的索引目录。

## 数据存储位置

- 默认：`Path.Combine(AppContext.BaseDirectory, "lucene-index")`
- 开启多租户：`lucene-index/{tenantId}/{indexName}`
- 可通过 `ZyknowLuceneOptions.IndexRootPath` 配置

## 单元测试

- Domain 测试：验证 Fluent 配置正确生成描述器
- Application 测试：索引示例实体并验证搜索行为

## 许可协议

MIT

## 索引同步（Synchronizer）

- 目标：保证 Lucene 索引与数据库一致，支持全量/增量修复。
- 服务：`LuceneIndexSynchronizer`（应用层），支持泛型主键 `SyncAllAsync<T,TKey>` 与按租户 `SyncTenantAsync(tenantId, repositoryFactory)`。
- 自动投影：
  - 对仅包含属性字段（`Field`）的实体，系统将“自动根据描述器”只加载必要列（Id + 已注册字段），不构造实体实例。
  - 使用字典文档工厂 `LuceneDocumentFactory.CreateDocument(IDictionary<string,string>, descriptor)` 构造文档后写入。
- 回退策略：
  - 若存在派生字段（`ValueField(Func<T,string>)`）且未声明依赖，自动回退为“完整实体加载+IndexRangeAsync”，保证正确性。
  - 同步器会记录日志：
    - 投影路径（Debug）示例：`Lucene sync using projection for {Entity} (Index:{IndexName}) Fields:{Fields}`
    - 回退路径（Information）示例：`Lucene sync fallback to entity load for {Entity} (Index:{IndexName}). ValueFields:{Fields}`
- 依赖声明（可选优化）：
  - 为派生字段使用 `Depends(() => x.Title).Depends(() => x.Description)` 声明所需列，后续可切换为自动投影。
- 自定义仓储（可选）：
  - `SyncTenantAsync` 通过 `repositoryFactory(Type entityType)` 接收具体仓储实例，遍历所有已注册实体进行同步。

### 示例：按租户同步

```csharp
await _synchronizer.SyncTenantAsync(
    tenantId: CurrentTenant.Id,
    repositoryFactory: type => type switch
    {
        var t when t == typeof(Comic) => ServiceProvider.GetRequiredService<IRepository<Comic, Guid>>(),
        var t when t == typeof(Video) => ServiceProvider.GetRequiredService<IRepository<Video, Guid>>(),
        _ => null
    },
    batchSize: 1000,
    deleteMissing: true
);
```

### 批量写入文档（字典）

```csharp
var values = new Dictionary<string, string>
{
    [descriptor.IdFieldName] = comic.Id.ToString(),
    ["Title"] = comic.Title,
    ["Description"] = comic.Description
};
var doc = LuceneDocumentFactory.CreateDocument(values, descriptor);
await _indexManager.IndexRangeDocumentsAsync(descriptor, new[] { doc }, replace: false);
```

## 字段配置方法详解（FieldBuilder）

在 `EntitySearchBuilder<T>.Field(expr, configure)` 或 `ValueField(selector, configure)` 的 `configure` 委托中，可使用以下方法配置该字段的写入与检索行为：

- `Store(bool enabled = true)`

  - 作用：是否在索引中“存储”字段原始值（用于搜索结果的展示或二次处理）。
  - 默认：`false`。调用 `f.Store()` 即开启。
  - 影响：开启后，`LuceneSearchAppService` 能从命中文档取出该字段值并放入结果的 `Payload`。

- `Name(string name)`

  - 作用：显式设置字段名。
  - 默认：属性字段使用属性名，`ValueField` 默认名为 `"Value"`。
  - 影响：索引与查询都以此名称为准，便于统一展示或约定。

- `Keyword()`

  - 作用：将字段按“关键字”索引（不分词），适合精确匹配的标识符、代码、标签等。
  - 默认：关闭。调用 `f.Keyword()` 使用 `StringField` 写入；否则使用 `TextField` 写入（参与分词）。

- `Autocomplete(int minGram = 1, int maxGram = 20)`

  - 作用：声明该字段用于自动补全的提示参数（NGram/EdgeNGram）。
  - 默认：不设置。
  - 说明：目前作为描述符层面的提示参数；要生效需配合自定义分词器/索引策略（例如使用 NGram 分词的 `AnalyzerFactory`）。

- `StoreTermVectors(bool positions = true, bool offsets = true)`

  - 作用：为字段存储词条向量信息（positions/offsets），常用于生成高亮等高级功能。
  - 默认：不设置。
  - 说明：目前为配置提示；如需启用高亮/片段生成，需在检索流程中结合 `TermVector` 使用或扩展索引写入类型。

- `Depends(Expression<Func<object>> member)`
  - 作用：为 `ValueField`（派生字段）声明其依赖的实体属性列，便于同步器只加载必要列并计算派生文本。
  - 默认：不设置。
  - 影响：同步器会优先使用“自动投影”仅加载 `Depends` 声明的列；未声明或存在不可投影委托时，回退为“完整实体加载”，并记录 Info 级日志。

### 配置示例

```csharp
model.Entity<Book>(e =>
{
    // 属性字段：参与分词并存储原始值
    e.Field(x => x.Title, f => f.Store());

    // 关键字段：不分词、精确匹配、重命名
    e.Field(x => x.Code, f => f.Keyword().Name("BookCode"));

    // 派生字段：声明依赖以启用仅加载必要列的自动投影
    e.ValueField(x => $"{x.Author} - {x.Title}", f =>
        f.Name("AuthorTitle").Store()
         .Depends(() => x.Author)
         .Depends(() => x.Title));
});
```

## 过滤扩展（FilterProvider）与 LINQ 映射

你可以在应用层通过 `ILuceneFilterProvider` 将自定义过滤接入搜索管线：

- 接口：`Task<Query?> BuildAsync(SearchFilterContext ctx)`
- 上下文：`{ string EntityName, EntitySearchDescriptor Descriptor, SearchQueryInput Input }`
- 组合方式：返回的过滤 `Query` 以 `Occur.MUST` 合并到最终查询

提供了一个轻量的 LINQ → Lucene 映射辅助，支持用简单 LINQ 写过滤表达式并转换为 Lucene 查询：

- 等值：`x => x.Field == value` → `TermQuery`
- IN：`values.Contains(x.Field)` → 多个 `TermQuery`，`BooleanQuery SHOULD + MinimumNumberShouldMatch=1`
- 前缀：`x => x.Field.StartsWith("ab")` → `PrefixQuery`
- 后缀通配：`x => x.Field.EndsWith(".jpg")` → `WildcardQuery("*.jpg")`
- 包含通配：`x => x.Field.Contains("foo")` → `WildcardQuery("*foo*")`
- 组合：`AndAlso` → `MUST`，`OrElse` → `SHOULD`

注意：

- 参与过滤的字段必须写入索引（精确匹配推荐 `Keyword()`）。
- 字段名解析来自 `Descriptor.Fields`；表达式中的成员名需与描述符字段名一致。
- 性能建议：优先使用前缀/关键字过滤，谨慎使用大范围通配符。

### 示例：按 LibraryId 过滤

```csharp
public class MediumLuceneFilter : ILuceneFilterProvider, IScopedDependency
{
    public Task<Query?> BuildAsync(SearchFilterContext ctx)
    {
        var ids = new [] { guid1, guid2 };
        Expression<Func<MediumProj, bool>> expr = x => ids.Contains(x.LibraryId);
        return Task.FromResult(LinqLucene.Where(ctx.Descriptor, expr));
    }
    private sealed class MediumProj { public Guid LibraryId { get; set; } }
}
```

### 范围查询（TermRangeQuery）

基于词项的范围比较按照“字符串字典序”进行。在索引阶段请先规范化数值/日期，确保排序正确。

- 数值：使用固定宽度的零填充字符串（如 8 位）

```csharp
// 索引为零填充字符串，例如 "00001234"
Expression<Func<ItemProj, bool>> expr = x => x.ReadCount >= "00000010" && x.ReadCount < "00000100";
var q = LinqLucene.Where(ctx.Descriptor, expr);
```

- 日期：使用可排序的格式，例如 `yyyyMMddHHmmss`

```csharp
// 将 CreatedAt 索引为 "20250101000000" 风格字符串
Expression<Func<ItemProj, bool>> expr = x => x.CreatedAt >= "20250101000000" && x.CreatedAt < "20260101000000";
var q = LinqLucene.Where(ctx.Descriptor, expr);
```

注意：

- 精确/规范化字段优先使用 `Keyword()/LowerCaseKeyword()`。
- 如需真正的数值/日期范围类型，可考虑扩展为 Numeric/Points 查询。

### 关键字大小写不敏感（文化）

通过 `LowerCaseKeyword()` 或 `LowerCaseKeyword(CultureInfo)` 在索引写入与查询映射阶段统一小写规范化：

```csharp
model.Entity<Tag>(e =>
{
    e.Field(x => x.Name, f => f.LowerCaseKeyword(new System.Globalization.CultureInfo("tr-TR")));
});
```

该配置会在索引写入与 LINQ → Lucene 映射（Term/Prefix/Wildcard/IN）时应用小写转换以确保匹配一致。
