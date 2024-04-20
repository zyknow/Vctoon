using System;
using System.Threading.Tasks;
using Vctoon.Comics;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Comics;

public class ComicRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly IComicRepository _comicRepository;

    public ComicRepositoryTests()
    {
        _comicRepository = GetRequiredService<IComicRepository>();
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
