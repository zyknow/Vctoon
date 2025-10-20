using System.Linq.Expressions;
using Lucene.Net.Search;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Zyknow.Abp.Lucene.Analyzers;
using Zyknow.Abp.Lucene.Application.Tests.Fakes;
using Zyknow.Abp.Lucene.Dtos;
using Zyknow.Abp.Lucene.Filtering;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Services;

namespace Zyknow.Abp.Lucene.Application.Tests;

public class NumericAndDateLinqTests
{
    [Fact]
    public async Task Int32_Range_And_Equality_Should_Work()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.PerTenantIndex = false;
            opt.IndexRootPath = Path.Combine(Path.GetTempPath(), "lucene-index-num-tests");
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.ConfigureLucene(model =>
            {
                model.Entity<Item>(e =>
                {
                    e.Field(x => x.Name, f => f.Store());
                    e.Field(x => x.Count, f => f.AsInt32());
                });
            });
        });
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAppService>();
        services.AddSingleton<ILuceneFilterProvider>(new CountRangeProvider(10, 100));
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        var search = sp.GetRequiredService<LuceneAppService>();

        await indexer.RebuildAsync(typeof(Item));
        await indexer.IndexRangeAsync(new List<Item>
        {
            new("1", "A", 5),
            new("2", "B", 20),
            new("3", "C", 100)
        }, true);

        var result = await search.SearchAsync("Item",
            new SearchQueryInput { Query = "A OR B OR C", MaxResultCount = 10, SkipCount = 0 });
        Assert.Equal(2, result.TotalCount); // 20 and 100 in range [10,100]
    }

    [Fact]
    public async Task DateEpochMillis_Range_Should_Work()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.PerTenantIndex = false;
            opt.IndexRootPath = Path.Combine(Path.GetTempPath(), "lucene-index-date-tests");
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.ConfigureLucene(model =>
            {
                model.Entity<Event>(e =>
                {
                    e.Field(x => x.Name, f => f.Store());
                    e.Field(x => x.OccurredAt, f => f.AsDateEpochMillis());
                });
            });
        });
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAppService>();
        services.AddSingleton<ILuceneFilterProvider>(new DateRangeProvider(DateTime.UtcNow.AddDays(-2),
            DateTime.UtcNow.AddDays(2)));
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        var search = sp.GetRequiredService<LuceneAppService>();

        var now = DateTime.UtcNow;
        await indexer.RebuildAsync(typeof(Event));
        await indexer.IndexRangeAsync(new List<Event>
        {
            new("1", "before", now.AddDays(-3)),
            new("2", "inside", now),
            new("3", "after", now.AddDays(3))
        }, true);

        var result = await search.SearchAsync("Event",
            new SearchQueryInput { Query = "before OR inside OR after", MaxResultCount = 10, SkipCount = 0 });
        Assert.Equal(1, result.TotalCount); // only inside is within range
    }

    private class CountRangeProvider(int minIncl, int maxIncl) : ILuceneFilterProvider
    {
        public Task<Query?> BuildAsync(SearchFilterContext ctx)
        {
            Expression<Func<ItemProj, bool>> expr = x => x.Count >= minIncl && x.Count <= maxIncl;
            var q = LinqLucene.Where(ctx.Descriptor, expr);
            return Task.FromResult<Query?>(q);
        }

        private class ItemProj
        {
            public int Count { get; set; }
        }
    }

    private class DateRangeProvider(DateTime minIncl, DateTime maxIncl) : ILuceneFilterProvider
    {
        public Task<Query?> BuildAsync(SearchFilterContext ctx)
        {
            Expression<Func<EventProj, bool>> expr = x => x.OccurredAt >= minIncl && x.OccurredAt <= maxIncl;
            var q = LinqLucene.Where(ctx.Descriptor, expr);
            return Task.FromResult<Query?>(q);
        }

        private class EventProj
        {
            public DateTime OccurredAt { get; set; }
        }
    }

    private record Item(string Id, string Name, int Count);

    private record Event(string Id, string Name, DateTime OccurredAt);
}