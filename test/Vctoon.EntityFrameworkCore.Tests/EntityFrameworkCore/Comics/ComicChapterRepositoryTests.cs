using Vctoon.Comics;

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