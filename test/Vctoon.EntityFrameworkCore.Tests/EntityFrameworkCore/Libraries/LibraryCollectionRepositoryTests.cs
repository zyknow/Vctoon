using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class LibraryCollectionRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly ILibraryCollectionRepository _libraryCollectionRepository;

    public LibraryCollectionRepositoryTests()
    {
        _libraryCollectionRepository = GetRequiredService<ILibraryCollectionRepository>();
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
