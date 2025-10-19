using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;
using Zyknow.Abp.Lucene.Options;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Microsoft.Extensions.Options;
using Volo.Abp.MultiTenancy;
using Zyknow.Abp.Lucene.Dtos;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Zyknow.Abp.Lucene.Filtering;

namespace Zyknow.Abp.Lucene.Services;

public class LuceneAppService : ApplicationService, ILuceneService
{
    private readonly LuceneOptions _options;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<LuceneAppService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<ILuceneFilterProvider> _filterProviders;

    public LuceneAppService(IOptions<LuceneOptions> options, ICurrentTenant currentTenant, ILogger<LuceneAppService> logger, IServiceProvider serviceProvider, IEnumerable<ILuceneFilterProvider> filterProviders)
    {
        _options = options.Value;
        _currentTenant = currentTenant;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _filterProviders = filterProviders;
    }

    public async Task<SearchResultDto> SearchAsync(string entityName, SearchQueryInput input)
    {
        await Task.Yield();
        var descriptor = _options.Descriptors.Values.FirstOrDefault(d => d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
            throw new BusinessException(code: "Lucene:EntityNotConfigured").WithData("Entity", entityName);

        var indexPath = GetIndexPath(descriptor.IndexName);
        _logger.LogInformation("Lucene search on index {Index} at path {Path}. Query={Query} Prefix={Prefix} Fuzzy={Fuzzy}", descriptor.IndexName, indexPath, input.Query, input.Prefix, input.Fuzzy);
        using var dir = _options.DirectoryFactory(indexPath);
        using var reader = DirectoryReader.Open(dir);

        var fields = descriptor.Fields.Where(f => f.Searchable).Select(f => f.Name).Distinct().ToArray();
        var analyzer = _options.AnalyzerFactory();
        var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, fields, analyzer)
        {
            DefaultOperator = _options.LuceneQuery.MultiFieldMode.Equals("AND", StringComparison.OrdinalIgnoreCase) ? Operator.AND : Operator.OR,
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
                    _logger.LogWarning("Lucene query variant parse failed: variant={Variant} error={Error}", v, ex.Message);
                }
            }
            finalQuery = bq;
        }

        // 执行过滤器（MUST 合并）
        var filterCtx = new SearchFilterContext(entityName, descriptor, input);
        var composed = new BooleanQuery { { finalQuery, Occur.MUST } };
        foreach (var provider in _filterProviders)
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
                        fq = (Query)method.Invoke(null, new object[] { descriptor, filterCtx.Expression })!;
                    }
                }
                catch { /* ignore conversion errors */ }
            }
            if (fq != null)
            {
                composed.Add(fq, Occur.MUST);
            }
        }

        var searcher = new IndexSearcher(reader);
        var topN = input.SkipCount + input.MaxResultCount;
        var hits = searcher.Search(composed, topN);
        _logger.LogInformation("Lucene search returned {Total} hits", hits.TotalHits);
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

    public async Task<int> RebuildIndexAsync(string entityName)
    {
        await Task.Yield();
        var descriptor = _options.Descriptors.Values.FirstOrDefault(d => d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
            throw new BusinessException(code: "Lucene:EntityNotConfigured").WithData("Entity", entityName);

        var indexPath = GetIndexPath(descriptor.IndexName);
        _logger.LogInformation("Lucene rebuild index {Index} at path {Path}", descriptor.IndexName, indexPath);
        using var dir = _options.DirectoryFactory(indexPath);
        var analyzer = _options.AnalyzerFactory();
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
        using var writer = new IndexWriter(dir, config);
        var beforeMaxDoc = writer.MaxDoc;
        writer.DeleteAll();
        writer.Commit();
        _logger.LogInformation("Lucene rebuild cleared {Before} docs from {Index}", beforeMaxDoc, descriptor.IndexName);
        return beforeMaxDoc;
    }

    public async Task<int> RebuildAndIndexAllAsync(string entityName, int batchSize = 1000)
    {
        await Task.Yield();
        var descriptor = _options.Descriptors.Values.FirstOrDefault(d => d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
            throw new BusinessException(code: "Lucene:EntityNotConfigured").WithData("Entity", entityName);

        var cleared = await RebuildIndexAsync(entityName);

        // 反射解析仓储 IRepository<TEntity, TKey>
        var entityType = descriptor.EntityType;
        var keyType = entityType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>))
            ?.GetGenericArguments().FirstOrDefault() ?? typeof(Guid);

        var repoType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
        var repository = _serviceProvider.GetService(repoType);
        if (repository is null)
        {
            _logger.LogWarning("Lucene rebuild+index: repository not found for entity {Entity}", entityType.FullName);
            return cleared;
        }

        // 调用 LuceneIndexSynchronizer.SyncAllAsync<T,TKey>
        var synchronizer = _serviceProvider.GetRequiredService<LuceneIndexSynchronizer>();
        var method = typeof(LuceneIndexSynchronizer)
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .First(m => m.Name == nameof(LuceneIndexSynchronizer.SyncAllAsync) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);
        var gmethod = method.MakeGenericMethod(entityType, keyType);
        _logger.LogInformation("Lucene rebuild+index: start SyncAll for {Entity} with batchSize {Batch}", entityType.FullName, batchSize);
        await (Task)gmethod.Invoke(synchronizer, new object?[] { repository, batchSize, true, default(System.Threading.CancellationToken) })!;
        _logger.LogInformation("Lucene rebuild+index: completed for {Entity}", entityType.FullName);

        return cleared;
    }

    protected virtual string GetIndexPath(string indexName)
    {
        var root = _options.IndexRootPath;
        if (_options.PerTenantIndex && _currentTenant.Id.HasValue)
        {
            return System.IO.Path.Combine(root, _currentTenant.Id.Value.ToString(), indexName);
        }
        return System.IO.Path.Combine(root, indexName);
    }

    public async Task<int> GetIndexDocumentCountAsync(string entityName)
    {
        await Task.Yield();
        var descriptor = _options.Descriptors.Values.FirstOrDefault(d => d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
            throw new BusinessException(code: "Lucene:EntityNotConfigured").WithData("Entity", entityName);
        var indexPath = GetIndexPath(descriptor.IndexName);
        using var dir = _options.DirectoryFactory(indexPath);
        using var reader = DirectoryReader.Open(dir);
        _logger.LogInformation("Lucene index {Index} at {Path} has {Count} docs", descriptor.IndexName, indexPath, reader.MaxDoc);
        return reader.MaxDoc;
    }

    public async Task<SearchResultDto> DumpIndexAsync(string entityName, int take = 10)
    {
        await Task.Yield();
        var descriptor = _options.Descriptors.Values.FirstOrDefault(d => d.IndexName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        if (descriptor is null)
            throw new BusinessException(code: "Lucene:EntityNotConfigured").WithData("Entity", entityName);

        var indexPath = GetIndexPath(descriptor.IndexName);
        using var dir = _options.DirectoryFactory(indexPath);
        using var reader = DirectoryReader.Open(dir);
        var searcher = new IndexSearcher(reader);

        var hits = new List<SearchHitDto>();
        var max = Math.Min(reader.MaxDoc, take);
        for (int docId = 0; docId < max; docId++)
        {
            var doc = searcher.Doc(docId);
            var payload = new Dictionary<string, string>();
            foreach (var f in descriptor.Fields.Where(f => f.Store))
            {
                payload[f.Name] = doc.Get(f.Name);
            }
            hits.Add(new SearchHitDto
            {
                EntityId = doc.Get(descriptor.IdFieldName) ?? string.Empty,
                Score = 0,
                Payload = payload
            });
        }
        _logger.LogInformation("Lucene dump index {Index}: take={Take} showed {Count} docs", descriptor.IndexName, take, hits.Count);
        return new SearchResultDto(reader.MaxDoc, hits);
    }
}