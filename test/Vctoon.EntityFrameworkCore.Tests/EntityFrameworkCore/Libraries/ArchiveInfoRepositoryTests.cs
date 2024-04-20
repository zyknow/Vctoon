using Vctoon.Libraries;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class ArchiveInfoRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly IArchiveInfoRepository _archiveInfoRepository;

    public ArchiveInfoRepositoryTests()
    {
        _archiveInfoRepository = GetRequiredService<IArchiveInfoRepository>();
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