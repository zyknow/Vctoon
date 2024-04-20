using Vctoon.Libraries;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class ArchiveInfoPathRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly IArchiveInfoPathRepository _archiveInfoPathRepository;

    public ArchiveInfoPathRepositoryTests()
    {
        _archiveInfoPathRepository = GetRequiredService<IArchiveInfoPathRepository>();
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