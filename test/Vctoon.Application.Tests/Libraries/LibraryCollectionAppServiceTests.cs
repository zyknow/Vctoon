using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Vctoon.Libraries;

public class LibraryCollectionAppServiceTests : VctoonApplicationTestBase
{
    private readonly ILibraryCollectionAppService _libraryCollectionAppService;

    public LibraryCollectionAppServiceTests()
    {
        _libraryCollectionAppService = GetRequiredService<ILibraryCollectionAppService>();
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

