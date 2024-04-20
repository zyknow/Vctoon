using System;
using System.Threading.Tasks;
using Vctoon.Comics;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Comics;

public class ComicChapterRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly IComicChapterRepository _comicChapterRepository;

    public ComicChapterRepositoryTests()
    {
        _comicChapterRepository = GetRequiredService<IComicChapterRepository>();
    }

    /*
    [Fact]
    public async Task Test1()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange

            // Act

            //Assert
        });
    }
    */
}
