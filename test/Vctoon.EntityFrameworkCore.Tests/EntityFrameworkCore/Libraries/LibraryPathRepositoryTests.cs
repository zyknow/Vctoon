using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class LibraryPathRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly ILibraryPathRepository _libraryPathRepository;

    public LibraryPathRepositoryTests()
    {
        _libraryPathRepository = GetRequiredService<ILibraryPathRepository>();
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
