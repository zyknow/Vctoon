using Vctoon.Libraries;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class TagGroupRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly ITagGroupRepository _tagGroupRepository;

    public TagGroupRepositoryTests()
    {
        _tagGroupRepository = GetRequiredService<ITagGroupRepository>();
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