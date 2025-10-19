using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Zyknow.Abp.Lucene.Fluent;
using Zyknow.Abp.Lucene.Options;
using Zyknow.Abp.Lucene.Descriptors;

namespace Zyknow.Abp.Lucene.Domain.Tests;

public class FieldBehaviorTests
{
    [Fact]
    public void LowerCaseKeyword_Should_Set_Keyword_And_Culture()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.ConfigureLucene(model =>
            {
                model.Entity<Book>(e =>
                {
                    e.Field(x => x.Title, f => f.Store());
                    e.Field(x => x.Code, f => f.LowerCaseKeyword());
                });
            });
        });

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<LuceneOptions>>().Value;
        var d = options.Descriptors[typeof(Book)];
        var code = Assert.Single(d.Fields, f => f.Name == nameof(Book.Code));
        Assert.True(code.Keyword);
        Assert.True(code.LowerCaseKeyword);
        Assert.NotNull(code.LowerCaseCulture);
    }

    [Fact]
    public void ValueField_Should_Default_Name_And_Be_Searchable()
    {
        var services = new ServiceCollection();
        services.Configure<LuceneOptions>(opt =>
        {
            opt.ConfigureLucene(model =>
            {
                model.Entity<Book>(e =>
                {
                    e.Field(x => x.Title, f => f.Store());
                    e.ValueField(x => $"{x.Author} - {x.Title}");
                });
            });
        });

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<LuceneOptions>>().Value;
        var d = options.Descriptors[typeof(Book)];
        var val = Assert.Single(d.Fields, f => f.ValueSelector != null);
        Assert.Equal("Value", val.Name);
        Assert.True(val.Searchable);
        Assert.False(val.Keyword);
        Assert.False(val.Store);
    }

    private record Book(string Title, string Author, string Code);
}