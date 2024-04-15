using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Vctoon.Libraries;

public class LibraryAppServiceTests : VctoonApplicationTestBase
{
    private readonly ILibraryAppService _libraryAppService;

    public LibraryAppServiceTests()
    {
        _libraryAppService = GetRequiredService<ILibraryAppService>();
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

