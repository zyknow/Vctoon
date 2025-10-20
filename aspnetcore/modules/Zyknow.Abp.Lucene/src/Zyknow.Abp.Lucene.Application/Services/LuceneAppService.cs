using System.Reflection;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Zyknow.Abp.Lucene.Descriptors;
using Zyknow.Abp.Lucene.Dtos;
using Zyknow.Abp.Lucene.Filtering;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Permissions;
using Directory = Lucene.Net.Store.Directory;

namespace Zyknow.Abp.Lucene.Services;

public class LuceneAppService(
    IOptions<LuceneOptions> options,
    ICurrentTenant currentTenant,
    ILogger<LuceneAppService> logger,
    IServiceProvider serviceProvider,
    IEnumerable<ILuceneFilterProvider> filterProviders)
    : ApplicationService, ILuceneService
{
    private readonly LuceneOptions _options = options.Value;

    [Authorize(ZyknowLucenePermissions.Search.Default)]
    public virtual async Task<SearchResultDto> SearchAsync(string entityName, SearchQueryInput input)
    {
        await Task.Yield();
        var descriptor =
            _options.Descriptors.Values.FirstOrDefault(d =>
                d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
        {
            throw new BusinessException("Lucene:EntityNotConfigured").WithData("Entity", entityName);
        }

        var indexPath = GetIndexPath(descriptor.IndexName);
        logger.LogInformation(
            "Lucene search on index {Index} at path {Path}. Query={Query} Prefix={Prefix} Fuzzy={Fuzzy}",
            descriptor.IndexName, indexPath, input.Query, input.Prefix, input.Fuzzy);
        using var dir = _options.DirectoryFactory(indexPath);
        using var reader = DirectoryReader.Open(dir);

        var fields = descriptor.Fields.Where(f => f.Searchable).Select(f => f.Name).Distinct().ToArray();
        var analyzer = _options.AnalyzerFactory();
        var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, fields, analyzer)
        {
            DefaultOperator = _options.LuceneQuery.MultiFieldMode.Equals("AND", StringComparison.OrdinalIgnoreCase)
                ? Operator.AND
                : Operator.OR,
            AllowLeadingWildcard = true
        };

        var variants = new List<string> { input.Query };
        if (input.Prefix)
        {
            variants.Add($"{input.Query}*");
        }

        if (input.Fuzzy)
        {
            variants.Add($"{input.Query}~{_options.LuceneQuery.FuzzyMaxEdits}");
        }

        Query finalQuery;
        if (variants.Count == 1)
        {
            finalQuery = parser.Parse(variants[0]);
        }
        else
        {
            var bq = new BooleanQuery();
            foreach (var v in variants)
            {
                try
                {
                    var q = parser.Parse(v);
                    bq.Add(q, Occur.SHOULD);
                }
                catch (ParseException ex)
                {
                    logger.LogWarning("Lucene query variant parse failed: variant={Variant} error={Error}", v,
                        ex.Message);
                }
            }

            finalQuery = bq;
        }

        // 执行过滤器（MUST 合并）
        var filterCtx = new SearchFilterContext(entityName, descriptor, input);
        var composed = new BooleanQuery { { finalQuery, Occur.MUST } };
        foreach (var provider in filterProviders)
        {
            var fq = await provider.BuildAsync(filterCtx);
            if (fq == null && filterCtx.Expression != null)
            {
                // 使用端直接提供了表达式时，尝试统一转换
                try
                {
                    var gtype = filterCtx.Expression.Type.GetGenericArguments().FirstOrDefault();
                    if (gtype != null)
                    {
                        var method = typeof(LinqLucene).GetMethod(nameof(LinqLucene.Where))!.MakeGenericMethod(gtype);
                        fq = (Query)method.Invoke(null, [descriptor, filterCtx.Expression])!;
                    }
                }
                catch
                {
                    /* ignore conversion errors */
                }
            }

            if (fq != null)
            {
                composed.Add(fq, Occur.MUST);
            }
        }

        var searcher = new IndexSearcher(reader);
        var topN = input.SkipCount + input.MaxResultCount;
        var hits = searcher.Search(composed, topN);
        logger.LogInformation("Lucene search returned {Total} hits", hits.TotalHits);
        var slice = hits.ScoreDocs.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        var results = new List<SearchHitDto>(slice.Count);
        foreach (var sd in slice)
        {
            var doc = searcher.Doc(sd.Doc);
            var payload = new Dictionary<string, string>();
            foreach (var f in descriptor.Fields.Where(f => f.Store))
            {
                payload[f.Name] = doc.Get(f.Name);
            }

            results.Add(new SearchHitDto
            {
                EntityId = doc.Get(descriptor.IdFieldName) ?? string.Empty,
                Score = sd.Score,
                Payload = payload
            });
        }

        return new SearchResultDto(hits.TotalHits, results);
    }

    // 新增：MultiReader 聚合多实体搜索
    [Authorize(ZyknowLucenePermissions.Search.Default)]
    public virtual async Task<SearchResultDto> SearchManyAsync(MultiSearchInput input)
    {
        await Task.Yield();

        var entityNames = input.Entities;

        if (entityNames == null || entityNames.Count == 0)
        {
            throw new BusinessException("Lucene:EmptyEntities");
        }

        // 收集描述符与 reader
        var descriptors = new List<EntitySearchDescriptor>();
        var readers = new List<IndexReader>();
        var dirHandles = new List<Directory>();

        try
        {
            foreach (var name in entityNames)
            {
                var d = _options.Descriptors.Values.FirstOrDefault(x =>
                    x.IndexName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (d == null)
                {
                    logger.LogWarning("Lucene multi search ignored unconfigured entity {Entity}", name);
                    continue;
                }

                var p = GetIndexPath(d.IndexName);

                if (!System.IO.Directory.Exists(p) || System.IO.Directory.GetFiles(p).Length == 0)
                {
                    logger.LogInformation("Lucene multi search skipped entity {Entity} with no index at {Path}", name,
                        p);
                    continue;
                }

                var dir = _options.DirectoryFactory(p);
                descriptors.Add(d);
                dirHandles.Add(dir);
                readers.Add(DirectoryReader.Open(dir));
            }

            // 若所有实体均未配置，则直接返回空结果
            if (descriptors.Count == 0)
            {
                logger.LogInformation("Lucene multi search: no configured entities found. Returning empty result.");
                return new SearchResultDto(0, new List<SearchHitDto>(0));
            }

            // 构建字段并集
            var fields = descriptors
                .SelectMany(d => d.Fields.Where(f => f.Searchable).Select(f => f.Name))
                .Distinct()
                .ToArray();

            var analyzer = _options.AnalyzerFactory();
            var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, fields, analyzer)
            {
                DefaultOperator = _options.LuceneQuery.MultiFieldMode.Equals("AND", StringComparison.OrdinalIgnoreCase)
                    ? Operator.AND
                    : Operator.OR,
                AllowLeadingWildcard = true
            };

            var variants = new List<string> { input.Query };
            if (input.Prefix)
            {
                variants.Add($"{input.Query}*");
            }

            if (input.Fuzzy)
            {
                variants.Add($"{input.Query}~{_options.LuceneQuery.FuzzyMaxEdits}");
            }

            Query finalQuery;
            if (variants.Count == 1)
            {
                finalQuery = parser.Parse(variants[0]);
            }
            else
            {
                var bq = new BooleanQuery();
                foreach (var v in variants)
                {
                    try
                    {
                        var q = parser.Parse(v);
                        bq.Add(q, Occur.SHOULD);
                    }
                    catch (ParseException ex)
                    {
                        logger.LogWarning("Lucene query variant parse failed: variant={Variant} error={Error}", v,
                            ex.Message);
                    }
                }

                finalQuery = bq;
            }

            // 过滤器：每个实体独立生成并合并
            var composed = new BooleanQuery { { finalQuery, Occur.MUST } };
            foreach (var d in descriptors)
            {
                var ctx = new SearchFilterContext(d.IndexName, d, input);
                foreach (var provider in filterProviders)
                {
                    var fq = await provider.BuildAsync(ctx);
                    if (fq != null)
                    {
                        composed.Add(fq, Occur.MUST);
                    }
                }
            }

            // MultiReader 全局检索
            var multi = new MultiReader(readers.ToArray(), true);
            var searcher = new IndexSearcher(multi);
            var topN = input.SkipCount + input.MaxResultCount;
            var hits = searcher.Search(composed, topN);
            logger.LogInformation("Lucene multi search returned {Total} hits across {Count} indexes", hits.TotalHits,
                readers.Count);
            var slice = hits.ScoreDocs.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            var results = new List<SearchHitDto>(slice.Count);
            foreach (var sd in slice)
            {
                var doc = searcher.Doc(sd.Doc);
                // 尝试读取来源实体名（通过字段并集无法直接得知），此处放入 Payload 特殊键
                var payload = new Dictionary<string, string>();
                foreach (var d in descriptors)
                {
                    foreach (var f in d.Fields.Where(f => f.Store))
                    {
                        var v = doc.Get(f.Name);
                        if (v != null && !payload.ContainsKey(f.Name))
                        {
                            payload[f.Name] = v;
                        }
                    }
                }

                payload["__IndexName"] = ResolveDocIndexName(descriptors, doc);

                // id 字段并不统一，优先从各描述符的 id 字段中读取第一个存在的
                var id = descriptors
                    .Select(d => doc.Get(d.IdFieldName))
                    .FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;

                results.Add(new SearchHitDto
                {
                    EntityId = id,
                    Score = sd.Score,
                    Payload = payload
                });
            }

            return new SearchResultDto(hits.TotalHits, results);
        }
        finally
        {
            foreach (var r in readers)
            {
                r.Dispose();
            }

            foreach (var dir in dirHandles)
            {
                dir.Dispose();
            }
        }
    }

    [Authorize(ZyknowLucenePermissions.Indexing.Rebuild)]
    public virtual async Task<int> RebuildIndexAsync(string entityName)
    {
        await Task.Yield();
        var descriptor =
            _options.Descriptors.Values.FirstOrDefault(d =>
                d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
        {
            throw new BusinessException("Lucene:EntityNotConfigured").WithData("Entity", entityName);
        }

        var indexPath = GetIndexPath(descriptor.IndexName);
        logger.LogInformation("Lucene rebuild index {Index} at path {Path}", descriptor.IndexName, indexPath);
        using var dir = _options.DirectoryFactory(indexPath);
        var analyzer = _options.AnalyzerFactory();
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
        using var writer = new IndexWriter(dir, config);
        var beforeMaxDoc = writer.MaxDoc;
        writer.DeleteAll();
        writer.Commit();
        logger.LogInformation("Lucene rebuild cleared {Before} docs from {Index}", beforeMaxDoc, descriptor.IndexName);
        return beforeMaxDoc;
    }

    [Authorize(ZyknowLucenePermissions.Indexing.Rebuild)]
    public virtual async Task<int> RebuildAndIndexAllAsync(string entityName, int batchSize = 1000)
    {
        await Task.Yield();
        var descriptor =
            _options.Descriptors.Values.FirstOrDefault(d =>
                d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
        {
            throw new BusinessException("Lucene:EntityNotConfigured").WithData("Entity", entityName);
        }

        var cleared = await RebuildIndexAsync(entityName);

        // 反射解析仓储 IRepository<TEntity, TKey>
        var entityType = descriptor.EntityType;
        var keyType = entityType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>))
            ?.GetGenericArguments().FirstOrDefault() ?? typeof(Guid);

        var repoType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
        var repository = serviceProvider.GetService(repoType);
        if (repository is null)
        {
            logger.LogWarning("Lucene rebuild+index: repository not found for entity {Entity}", entityType.FullName);
            return cleared;
        }

        // 调用 LuceneIndexSynchronizer.SyncAllAsync<T,TKey>
        var synchronizer = serviceProvider.GetRequiredService<LuceneIndexSynchronizer>();
        var method = typeof(LuceneIndexSynchronizer)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(m => m.Name == nameof(LuceneIndexSynchronizer.SyncAllAsync) && m.IsGenericMethodDefinition &&
                        m.GetGenericArguments().Length == 2);
        var gmethod = method.MakeGenericMethod(entityType, keyType);
        logger.LogInformation("Lucene rebuild+index: start SyncAll for {Entity} with batchSize {Batch}",
            entityType.FullName, batchSize);
        await (Task)gmethod.Invoke(synchronizer,
            [repository, batchSize, true, CancellationToken.None])!;
        logger.LogInformation("Lucene rebuild+index: completed for {Entity}", entityType.FullName);

        return cleared;
    }

    [Authorize(ZyknowLucenePermissions.Indexing.Default)]
    public virtual async Task<int> GetIndexDocumentCountAsync(string entityName)
    {
        await Task.Yield();
        var descriptor =
            _options.Descriptors.Values.FirstOrDefault(d =>
                d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
        {
            throw new BusinessException("Lucene:EntityNotConfigured").WithData("Entity", entityName);
        }

        var indexPath = GetIndexPath(descriptor.IndexName);
        using var dir = _options.DirectoryFactory(indexPath);
        using var reader = DirectoryReader.Open(dir);
        logger.LogInformation("Lucene index {Index} at {Path} has {Count} docs", descriptor.IndexName, indexPath,
            reader.MaxDoc);
        return reader.MaxDoc;
    }

    private static string ResolveDocIndexName(IEnumerable<EntitySearchDescriptor> descriptors, Document doc)
    {
        // 简单策略：匹配哪个描述符的存储字段能唯一识别来源；若无法唯一，则返回 Unknown
        foreach (var d in descriptors)
        {
            var hasAny = d.Fields.Any(f => f.Store && doc.Get(f.Name) != null);
            if (hasAny)
            {
                return d.IndexName;
            }
        }

        return "Unknown";
    }

    protected virtual string GetIndexPath(string indexName)
    {
        var root = _options.IndexRootPath;
        if (_options.PerTenantIndex && currentTenant.Id.HasValue)
        {
            return Path.Combine(root, currentTenant.Id.Value.ToString(), indexName);
        }

        return Path.Combine(root, indexName);
    }
}