using Vctoon.Libraries;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class ImageFileRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly IImageFileRepository _imageFileRepository;

    public ImageFileRepositoryTests()
    {
        _imageFileRepository = GetRequiredService<IImageFileRepository>();
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