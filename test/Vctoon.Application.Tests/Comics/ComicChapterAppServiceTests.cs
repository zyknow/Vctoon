namespace Vctoon.Comics;

public class ComicChapterAppServiceTests : VctoonApplicationTestBase
{
    private readonly IComicChapterAppService _comicChapterAppService;

    public ComicChapterAppServiceTests()
    {
        _comicChapterAppService = GetRequiredService<IComicChapterAppService>();
    }

    /*
    [Fact]
    public async Task Test1()
    {
        // Arrange

        // Act

        // Assert
    }
    */
}