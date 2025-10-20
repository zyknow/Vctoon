# Zyknow.Abp.Lucene

Lucene.NET integration module for ABP Framework. Provides fluent configuration to index entity fields and a search application service.

## Features

- Global ICU-based analyzer (multilingual)
- Fluent API to select fields, keyword fields, boost, autocomplete
- Multi-tenant index isolation (optional)
- Index manager: add/update/delete/rebuild
- Search service: multi-field query, paging, payload

## Installation

- Add project references to `Zyknow.Abp.Lucene.*` as needed
- Register module in your host application's module dependencies

## Quick Start

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

## Indexing

```csharp
await _indexManager.IndexAsync(book);
await _indexManager.IndexRangeAsync(books, replace: true);
await _indexManager.DeleteAsync<Book>(bookId);
await _indexManager.RebuildAsync(typeof(Book));
```

## Event-Driven Auto Indexing

- Enable/disable: controlled via `ZyknowLuceneOptions.EnableAutoIndexingEvents` (default: true).
- How it works: subscribes to ABP local entity events (create/update/delete) and aggregates changes within the current UnitOfWork. It runs batch indexing on transaction commit (reduces writer open/commit cycles).
- Only configured entities: works only for types registered via Fluent DSL (`Descriptors`). Unregistered entities are ignored.
- Batch delete optimization: uses `DeleteRangeAsync<T>(IEnumerable<object> ids)` for faster deletion.
- Multi-tenancy: runs under the current tenant context and writes to the tenant-isolated index directory.

## Searching

HTTP GET endpoints are available for both single-entity and multi-entity aggregated search.

- Single-entity search: `GET /api/lucene/search/{entity}`
  - Query params (`SearchQueryInput`): `query`, `fuzzy`, `prefix`, `highlight`, `skipCount`, `maxResultCount`, `sorting`
  - Example:
    ```bash
    curl -G "http://localhost:5000/api/lucene/search/Book" \
      --data-urlencode "query=Lucene" \
      --data-urlencode "fuzzy=true" \
      --data-urlencode "prefix=true" \
      --data-urlencode "skipCount=0" \
      --data-urlencode "maxResultCount=10"
    ```

- Multi-entity aggregated search: `GET /api/lucene/search-many`
  - Query params (`MultiSearchInput`): `entities` (required, array), plus `query`, `fuzzy`, `prefix`, `highlight`, `skipCount`, `maxResultCount`, `sorting`
  - How to pass arrays: repeat the same key, e.g. `entities=Book&entities=Item`
  - Example:
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

Response
- Both endpoints return `SearchResultDto` (paged): `TotalCount` and `Items`.
- Each hit (`SearchHitDto`) includes:
  - `EntityId`: document key (from descriptor `IdFieldName`)
  - `Score`: relevance score
  - `Payload`: stored field key-values. In multi-entity mode, `Payload["__IndexName"]` indicates the source entity index name.

Notes
- Global query behavior is controlled by `LuceneOptions.LuceneQuery`:
  - `MultiFieldMode`: `AND`/`OR` (default `OR`).
  - `FuzzyMaxEdits`: Levenshtein edit distance for fuzzy matching (default 1).
- With multi-tenancy (`PerTenantIndex`), requests search under the current tenant's index directory.

## Storage Location

- Default: `Path.Combine(AppContext.BaseDirectory, "lucene-index")`
- With multi-tenancy: `lucene-index/{tenantId}/{indexName}`
- Configurable via `ZyknowLuceneOptions.IndexRootPath`

## Tests

- Domain tests validate fluent configuration
- Application tests index sample entities and assert search behavior

## License

MIT

## Synchronizer

- Purpose: keep Lucene index consistent with the database (full/incremental fixes).
- Service: `LuceneIndexSynchronizer` (Application layer), supports generic key `SyncAllAsync<T,TKey>` and per-tenant `SyncTenantAsync(tenantId, repositoryFactory)`.
- Automatic projection:
  - For entities with only property fields (`Field`), the synchronizer automatically loads only necessary columns (Id + registered fields) based on descriptors; no entity construction is needed.
  - Uses the dictionary document factory `LuceneDocumentFactory.CreateDocument(IDictionary<string,string>, descriptor)` to build documents and write.
- Fallback policy:
  - If there are derived fields (`ValueField(Func<T,string>)`) without declared dependencies, it falls back to “load full entity + IndexRangeAsync” to ensure correctness.
  - Logs:
    - Projection (Debug): `Lucene sync using projection for {Entity} (Index:{IndexName}) Fields:{Fields}`
    - Fallback (Information): `Lucene sync fallback to entity load for {Entity} (Index:{IndexName}). ValueFields:{Fields}`
- Dependency declaration (optional optimization):
  - Use `Depends(() => x.Title).Depends(() => x.Description)` for derived fields to later switch to automatic projection.
- Custom repositories (optional):
  - `SyncTenantAsync` accepts `repositoryFactory(Type entityType)` to provide repository instances and loops through all registered entities.

### Example: per-tenant sync
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

### Batch write documents (dictionary)
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

## FieldBuilder Configure Methods

Within `EntitySearchBuilder<T>.Field(expr, configure)` or `ValueField(selector, configure)`, you can use the following methods to control how the field is indexed and retrieved:

- `Store(bool enabled = true)`
  - Purpose: whether to store the original field value in the index (for result payload/display).
  - Default: `false`. Call `f.Store()` to enable.
  - Effect: when enabled, `LuceneSearchAppService` retrieves this field from the hit document and includes it in the result payload.

- `Name(string name)`
  - Purpose: set an explicit field name.
  - Default: property fields use the property name; `ValueField` defaults to `"Value"`.
  - Effect: index and queries use this name, helpful for consistent display/conventions.

- `Keyword()`
  - Purpose: index as a keyword (no tokenization), suitable for identifiers, codes, exact tags.
  - Default: off. When called, uses `StringField`; otherwise uses `TextField` (tokenized).

- `Autocomplete(int minGram = 1, int maxGram = 20)`
  - Purpose: declare hint parameters for autocomplete (NGram/EdgeNGram).
  - Default: not set.
  - Note: acts as a descriptor-level hint; requires a compatible analyzer/indexing strategy (e.g., NGram analyzer) to take effect.

- `StoreTermVectors(bool positions = true, offsets = true)`
  - Purpose: store term vectors (positions/offsets) for advanced features like highlighting.
  - Default: not set.
  - Note: currently a configuration hint; to enable highlighting/snippet generation, combine with search-time processing or extend index writing.

- `Depends(Expression<Func<object>> member)`
  - Purpose: declare dependent entity properties for `ValueField` (derived text), enabling the synchronizer to load only necessary columns.
  - Default: not set.
  - Effect: the synchronizer prioritizes automatic projection to load only declared columns; if missing or non-projectable delegates exist, it falls back to full entity loading and logs at Information level.

### Example
```csharp
model.Entity<Book>(e =>
{
    // Property field: tokenized and stored
    e.Field(x => x.Title, f => f.Store());

    // Keyword field: exact match and renamed
    e.Field(x => x.Code, f => f.Keyword().Name("BookCode"));

    // Derived field: declare dependencies to enable automatic projection
    e.ValueField(x => $"{x.Author} - {x.Title}", f =>
        f.Name("AuthorTitle").Store()
         .Depends(() => x.Author)
         .Depends(() => x.Title));
});
```

## Filter Provider & LINQ Query Mapping

You can plug custom filters into the search pipeline via `ILuceneFilterProvider` in the Application layer.

- Interface: `Task<Query?> BuildAsync(SearchFilterContext ctx)`
- Context: `{ string EntityName, EntitySearchDescriptor Descriptor, SearchQueryInput Input }`
- Composition: returned queries are added with `Occur.MUST`

Linq-to-Lucene helper allows writing filters using simple LINQ and maps them to Lucene queries:

- Equality: `x => x.Field == value` → `TermQuery`
- IN: `values.Contains(x.Field)` → multiple `TermQuery` with `BooleanQuery SHOULD + MinimumNumberShouldMatch=1`
- Prefix: `x => x.Field.StartsWith("ab")` → `PrefixQuery`
- Wildcard suffix: `x => x.Field.EndsWith(".jpg")` → `WildcardQuery("*.jpg")`
- Wildcard contains: `x => x.Field.Contains("foo")` → `WildcardQuery("*foo*")`
- Boolean composition: `AndAlso` → `MUST`, `OrElse` → `SHOULD`

Notes:
- Filter fields must be indexed (prefer `Keyword()` for exact match).
- Field name resolution uses `Descriptor.Fields`; expression member names must match descriptor field names.
- For performance/stability, prefer prefix/keyword over broad wildcards.

### Example: filter by LibraryId
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

### Range queries

Term-based range queries compare strings lexicographically. Normalize numbers/dates before indexing to ensure correct ordering.

- Numbers: zero-pad to a fixed width (e.g., 8 digits)
```csharp
// Index as zero-padded strings like "00001234"
Expression<Func<ItemProj, bool>> expr = x => x.ReadCount >= "00000010" && x.ReadCount < "00000100";
var q = LinqLucene.Where(ctx.Descriptor, expr);
```

- Dates: use a sortable format like `yyyyMMddHHmmss`
```csharp
// Index CreatedAt as "20250101000000" style strings
Expression<Func<ItemProj, bool>> expr = x => x.CreatedAt >= "20250101000000" && x.CreatedAt < "20260101000000";
var q = LinqLucene.Where(ctx.Descriptor, expr);
```

Notes:
- Prefer `Keyword()/LowerCaseKeyword()` for exact/normalized fields.
- For true numeric/date range types, consider extending to numeric/point queries.

### Case-insensitive keyword (culture)

Use `LowerCaseKeyword()` or `LowerCaseKeyword(CultureInfo)` to normalize values at index/write time and query-time mapping:
```csharp
model.Entity<Tag>(e =>
{
    e.Field(x => x.Name, f => f.LowerCaseKeyword(new System.Globalization.CultureInfo("tr-TR")));
});
```
This applies lowercasing during indexing and in LINQ → Lucene mapping (Term/Prefix/Wildcard/IN) for matching.
