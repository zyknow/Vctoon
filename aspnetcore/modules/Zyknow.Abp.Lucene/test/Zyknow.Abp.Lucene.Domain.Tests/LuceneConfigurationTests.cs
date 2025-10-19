using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Zyknow.Abp.Lucene.Options;

namespace Zyknow.Abp.Lucene.Domain.Tests;

public class LuceneConfigurationTests
{
    [Fact]
    public async Task Should_Build_Descriptors_With_Fluent_Api()
    {
        // Setup options
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
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

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<LuceneOptions>>().Value;
        Assert.True(options.Descriptors.ContainsKey(typeof(Book)));
        var d = options.Descriptors[typeof(Book)];
        Assert.Equal("Book", d.IndexName);
        Assert.Equal(3, d.Fields.Count);
    }

    private record Book(string Title, string Author, string Code);
}