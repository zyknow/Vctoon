using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Vctoon.Libraries;

public class ImageFileAppServiceTests : VctoonApplicationTestBase
{
    private readonly IImageFileAppService _imageFileAppService;

    public ImageFileAppServiceTests()
    {
        _imageFileAppService = GetRequiredService<IImageFileAppService>();
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

