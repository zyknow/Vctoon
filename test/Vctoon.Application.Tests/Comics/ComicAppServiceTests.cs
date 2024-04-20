using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Vctoon.Comics;

public class ComicAppServiceTests : VctoonApplicationTestBase
{
    private readonly IComicAppService _comicAppService;

    public ComicAppServiceTests()
    {
        _comicAppService = GetRequiredService<IComicAppService>();
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

