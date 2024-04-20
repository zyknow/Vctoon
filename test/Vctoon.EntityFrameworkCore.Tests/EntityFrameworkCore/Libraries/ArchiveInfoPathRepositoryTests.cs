using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

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
