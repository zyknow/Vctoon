using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Zyknow.Abp.Lucene.Analyzers;
using Zyknow.Abp.Lucene.Application.Tests.Fakes;
using Zyknow.Abp.Lucene.Dtos;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Services;

namespace Zyknow.Abp.Lucene.Application.Tests;

public class SearchIntegrationTests
{
    [Fact]
    public async Task Index_And_Search_Should_Return_Matched_Document()
    {
        // Arrange DI and options
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
        services.Configure<LuceneOptions>(opt =>
        {
            opt.PerTenantIndex = false;
            opt.IndexRootPath = Path.Combine(Path.GetTempPath(), "lucene-index-test");
            opt.AnalyzerFactory = AnalyzerFactories.IcuGeneral;
            opt.ConfigureLucene(model =>
            {
                model.Entity<Book>(e =>
                {
                    e.Field(x => x.Title, f => f.Name(nameof(Book.Title)).Store());
                    e.Field(x => x.Author, f => f.Name(nameof(Book.Author)).Store());
                    e.Field(x => x.Code, f => f.Name(nameof(Book.Code)).Keyword());
                });
            });
        });
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAppService>();

        var sp = services.BuildServiceProvider();
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        var search = sp.GetRequiredService<LuceneAppService>();

        var books = new List<Book>
        {
            new("1", "Lucene in Action", "Erik Hatcher", "B001"),
            new("2", "Pro .NET Lucene", "John Doe", "B002")
        };

        // Act: index documents
        await indexer.IndexRangeAsync(books, true);

        // Act: search
        var result = await search.SearchAsync("Book", new SearchQueryInput
        {
            Query = "Lucene",
            SkipCount = 0,
            MaxResultCount = 10
        });

        // Assert
        Assert.True(result.TotalCount > 0);
        Assert.Contains(result.Items, i => i.Payload.Values.Any(v => v.Contains("Lucene")));
    }

    private record Book(string Id, string Title, string Author, string Code);
}