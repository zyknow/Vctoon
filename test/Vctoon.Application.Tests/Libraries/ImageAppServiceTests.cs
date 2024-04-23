namespace Vctoon.Libraries;

public class ImageAppServiceTests : VctoonApplicationTestBase
{
    private readonly IImageFileAppService _imageFileAppService;

    public ImageAppServiceTests()
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