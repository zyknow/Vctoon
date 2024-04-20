using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class LibraryRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly ILibraryRepository _libraryRepository;

    public LibraryRepositoryTests()
    {
        _libraryRepository = GetRequiredService<ILibraryRepository>();
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
