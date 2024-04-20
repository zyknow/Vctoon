using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

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
