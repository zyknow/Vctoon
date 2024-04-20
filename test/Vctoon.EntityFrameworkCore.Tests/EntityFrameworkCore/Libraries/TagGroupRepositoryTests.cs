using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

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
