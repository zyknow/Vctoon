using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Zyknow.Abp.Lucene.Analyzers;
using Zyknow.Abp.Lucene.Dtos;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Services;
using Zyknow.Abp.Lucene.Controllers;
using Volo.Abp.MultiTenancy;

namespace Zyknow.Abp.Lucene.HttpApi.Tests;

public class LuceneControllerTests
{
    [Fact]
    public async Task Search_Endpoint_Should_Return_Hits()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddControllers().AddApplicationPart(typeof(LuceneController).Assembly);
                services.AddSingleton<ICurrentTenant>(new FakeCurrentTenant { Id = null });
                services.Configure<LuceneOptions>(opt =>
                {
                    opt.IndexRootPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lucene-index-http-tests");
                    opt.PerTenantIndex = false;
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
                services.AddSingleton<LuceneIndexManager>();
                services.AddSingleton<ILuceneService, LuceneAppService>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });

        using var server = new TestServer(builder);
        var sp = server.Services;
        // prepare index
        var indexer = sp.GetRequiredService<LuceneIndexManager>();
        await indexer.RebuildAsync(typeof(Book));
        await indexer.IndexRangeAsync(new[]
        {
            new Book("1", "Lucene in Action", "Erik", "B001"),
            new Book("2", "Pro .NET Lucene", "John", "B002")
        }, replace: true);

        var client = server.CreateClient();
        var payload = JsonSerializer.Serialize(new SearchQueryInput { Query = "Lucene", MaxResultCount = 10, SkipCount = 0 });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/api/lucene/search/Book", content);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SearchResultDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        Assert.True(result.TotalCount >= 1);
    }

    private record Book(string Id, string Title, string Author, string Code);
    private sealed class FakeCurrentTenant : ICurrentTenant
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId => null;
        public bool IsAvailable => Id.HasValue;
        public IDisposable Change(Guid? id, string? name = null) => new NoopDisposable(() => { Id = id; Name = name ?? string.Empty; });

        private sealed class NoopDisposable : IDisposable
        {
            private readonly Action _onDispose;
            public NoopDisposable(Action onDispose) => _onDispose = onDispose;
            public void Dispose() { _onDispose(); }
        }
    }
}