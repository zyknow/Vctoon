using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Zyknow.Abp.Lucene.Descriptors;
using Zyknow.Abp.Lucene.Indexing;
using Zyknow.Abp.Lucene.Options;
using Lucene.Net.Documents;

namespace Zyknow.Abp.Lucene.Services;

public class LuceneIndexManager
{
    private readonly LuceneOptions _options;
    private readonly ICurrentTenant _currentTenant;

    public LuceneIndexManager(IOptions<LuceneOptions> options, ICurrentTenant currentTenant)
    {
        _options = options.Value;
        _currentTenant = currentTenant;
    }

    public Task IndexAsync<T>(T entity)
    {
        var descriptor = GetDescriptor(typeof(T));
        Write(descriptor, writer =>
        {
            var doc = LuceneDocumentFactory.CreateDocument(entity!, descriptor);
            writer.UpdateDocument(new Term(descriptor.IdFieldName, doc.Get(descriptor.IdFieldName)), doc);
        });
        return Task.CompletedTask;
    }

    public Task IndexRangeAsync<T>(IEnumerable<T> entities, bool replace = false)
    {
        var descriptor = GetDescriptor(typeof(T));
        Write(descriptor, writer =>
        {
            if (replace)
            {
                writer.DeleteAll();
            }
            foreach (var entity in entities)
            {
                var doc = LuceneDocumentFactory.CreateDocument(entity!, descriptor);
                writer.UpdateDocument(new Term(descriptor.IdFieldName, doc.Get(descriptor.IdFieldName)), doc);
            }
        });
        return Task.CompletedTask;
    }

    public Task IndexRangeDocumentsAsync(Descriptors.EntitySearchDescriptor descriptor, IEnumerable<Document> documents, bool replace = false)
    {
        Write(descriptor, writer =>
        {
            if (replace)
            {
                writer.DeleteAll();
            }
            foreach (var doc in documents)
            {
                var id = doc.Get(descriptor.IdFieldName);
                writer.UpdateDocument(new Term(descriptor.IdFieldName, id), doc);
            }
        });
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(object id)
    {
        var descriptor = GetDescriptor(typeof(T));
        Write(descriptor, writer =>
        {
            writer.DeleteDocuments(new Term(descriptor.IdFieldName, id.ToString()));
        });
        return Task.CompletedTask;
    }

    public Task RebuildAsync(Type? entityType = null)
    {
        if (entityType == null)
        {
            foreach (var d in _options.Descriptors.Values)
            {
                Write(d, writer => writer.DeleteAll());
            }
        }
        else
        {
            var d = GetDescriptor(entityType);
            Write(d, writer => writer.DeleteAll());
        }
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync<T>(IEnumerable<object> ids)
    {
        var descriptor = GetDescriptor(typeof(T));
        Write(descriptor, writer =>
        {
            foreach (var id in ids)
            {
                writer.DeleteDocuments(new Term(descriptor.IdFieldName, id.ToString()));
            }
        });
        return Task.CompletedTask;
    }

    protected virtual EntitySearchDescriptor GetDescriptor(Type type)
    {
        if (!_options.Descriptors.TryGetValue(type, out var descriptor))
            throw new BusinessException(code: "Lucene:EntityNotConfigured").WithData("Entity", type.FullName);
        return descriptor;
    }

    protected virtual void Write(EntitySearchDescriptor descriptor, Action<IndexWriter> action)
    {
        var indexPath = GetIndexPath(descriptor.IndexName);
        using var dir = _options.DirectoryFactory(indexPath);
        var analyzer = _options.AnalyzerFactory();
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
        using var writer = new IndexWriter(dir, config);
        action(writer);
        writer.Commit();
    }

    protected internal virtual string GetIndexPath(string indexName)
    {
        var root = _options.IndexRootPath;
        if (_options.PerTenantIndex && _currentTenant.Id.HasValue)
        {
            return System.IO.Path.Combine(root, _currentTenant.Id.Value.ToString(), indexName);
        }
        return System.IO.Path.Combine(root, indexName);
    }
}