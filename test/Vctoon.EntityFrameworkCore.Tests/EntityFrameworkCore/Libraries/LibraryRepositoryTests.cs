using Vctoon.Libraries;

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