using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

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
