using System.Linq.Expressions;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Zyknow.Abp.Lucene.Analyzers;
using Zyknow.Abp.Lucene.Application.Tests.Fakes;
using Zyknow.Abp.Lucene.Dtos;
using Zyknow.Abp.Lucene.Filtering;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Services;

namespace Zyknow.Abp.Lucene.Application.Tests;

public class FilteringAndQueryTests
{
    [Fact]
    public async Task Search_With_Prefix_And_Fuzzy_Should_Return_Matches()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.PerTenantIndex = false;
            opt.IndexRootPath = Path.Combine(Path.GetTempPath(), "lucene-index-tests");
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.LuceneQuery.FuzzyMaxEdits = 1;
            opt.LuceneQuery.MultiFieldMode = "OR";
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
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAppService>();
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
        services.AddLogging();

        var sp = services.BuildServiceProvider();
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        var search = sp.GetRequiredService<LuceneAppService>();

        var books = new List<Book>
        {
            new("1", "Lucene in Action", "Erik Hatcher", "B001"),
            new("2", "Pro .NET Lucene", "John Doe", "B002")
        };
        await indexer.RebuildAsync(typeof(Book));
        await indexer.IndexRangeAsync(books, true);

        var prefixResult = await search.SearchAsync("Book", new SearchQueryInput
        {
            Query = "Lucen",
            MaxResultCount = 10,
            SkipCount = 0,
            Prefix = true,
            Fuzzy = false
        });
        Assert.True(prefixResult.TotalCount > 0);

        var fuzzyResult = await search.SearchAsync("Book", new SearchQueryInput
        {
            Query = "Lucne", // one edit
            MaxResultCount = 10,
            SkipCount = 0,
            Prefix = false,
            Fuzzy = true
        });
        Assert.True(fuzzyResult.TotalCount > 0);
    }

    [Fact]
    public async Task FilterProvider_Should_Restrict_To_Specific_Code()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.PerTenantIndex = false;
            opt.IndexRootPath = Path.Combine(Path.GetTempPath(), "lucene-index-tests");
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.ConfigureLucene(model =>
            {
                model.Entity<Book>(e =>
                {
                    e.Field(x => x.Title, f => f.Store());
                    e.Field(x => x.Code, f => f.Keyword());
                });
            });
        });
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAppService>();
        services.AddSingleton<ILuceneFilterProvider>(new RestrictCodeProvider("B001"));
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
        services.AddLogging();

        var sp = services.BuildServiceProvider();
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        var search = sp.GetRequiredService<LuceneAppService>();

        await indexer.RebuildAsync(typeof(Book));
        await indexer.IndexRangeAsync(new List<Book>
        {
            new("1", "Lucene in Action", "Erik", "B001"),
            new("2", "Lucene for .NET", "John", "B002")
        }, true);

        var result = await search.SearchAsync("Book",
            new SearchQueryInput { Query = "Lucene", MaxResultCount = 10, SkipCount = 0 });
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Linq_Contains_With_LowerCase_Should_Match_Case_Insensitive()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.PerTenantIndex = false;
            opt.IndexRootPath = Path.Combine(Path.GetTempPath(), "lucene-index-tests");
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.ConfigureLucene(model =>
            {
                model.Entity<Book>(e =>
                {
                    e.Field(x => x.Title, f => f.Store());
                    e.Field(x => x.Code, f => f.LowerCaseKeyword());
                });
            });
        });
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAppService>();
        services.AddSingleton<ILuceneFilterProvider>(new CodesInProvider(["B001", "B002"]));
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
        services.AddLogging();

        var sp = services.BuildServiceProvider();
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        var search = sp.GetRequiredService<LuceneAppService>();

        await indexer.RebuildAsync(typeof(Book));
        await indexer.IndexRangeAsync(new List<Book>
        {
            new("1", "Lucene", "A", "b001"),
            new("2", "Lucene", "B", "B002")
        }, true);

        var result = await search.SearchAsync("Book",
            new SearchQueryInput { Query = "Lucene", MaxResultCount = 10, SkipCount = 0 });
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task RangeQuery_Should_Filter_By_Normalized_Numeric()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.PerTenantIndex = false;
            opt.IndexRootPath = Path.Combine(Path.GetTempPath(), "lucene-index-tests");
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.ConfigureLucene(model =>
            {
                model.Entity<Item>(e =>
                {
                    e.Field(x => x.Name, f => f.Store());
                    e.ValueField(x => x.ReadCount.ToString("00000000"), f => f.Name("ReadCountNorm").Keyword());
                });
            });
        });
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAppService>();
        services.AddSingleton<ILuceneFilterProvider>(new ReadCountRangeProvider("00000010", "00000050"));
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
        services.AddLogging();

        var sp = services.BuildServiceProvider();
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        var search = sp.GetRequiredService<LuceneAppService>();

        await indexer.RebuildAsync(typeof(Item));
        await indexer.IndexRangeAsync(new List<Item>
        {
            new("i1", "dummy", 5),
            new("i2", "dummy", 20)
        }, true);

        var result = await search.SearchAsync("Item",
            new SearchQueryInput { Query = "dummy", MaxResultCount = 10, SkipCount = 0 });
        Assert.Equal(1, result.TotalCount);
    }

    private class RestrictCodeProvider(string code) : ILuceneFilterProvider
    {
        public Task<Query?> BuildAsync(SearchFilterContext ctx)
        {
            var q = new TermQuery(new Term("Code", code));
            return Task.FromResult<Query?>(q);
        }
    }

    private class CodesInProvider(IEnumerable<string> codes) : ILuceneFilterProvider
    {
        private readonly HashSet<string> _codes = [..codes];

        public Task<Query?> BuildAsync(SearchFilterContext ctx)
        {
            Expression<Func<BookProj, bool>> expr = x => _codes.Contains(x.Code);
            var q = LinqLucene.Where(ctx.Descriptor, expr);
            return Task.FromResult<Query?>(q);
        }

        private class BookProj
        {
            public string Code { get; set; } = string.Empty;
        }
    }

    private class ReadCountRangeProvider(string minIncl, string maxExcl) : ILuceneFilterProvider
    {
        public Task<Query?> BuildAsync(SearchFilterContext ctx)
        {
            var q = new TermRangeQuery("ReadCountNorm",
                new BytesRef(minIncl),
                new BytesRef(maxExcl),
                true,
                false);
            return Task.FromResult<Query?>(q);
        }

        private class ItemProj
        {
            public string ReadCountNorm { get; set; } = string.Empty;
        }
    }

    private record Book(string Id, string Title, string Author, string Code);

    private record Item(string Id, string Name, int ReadCount);
}