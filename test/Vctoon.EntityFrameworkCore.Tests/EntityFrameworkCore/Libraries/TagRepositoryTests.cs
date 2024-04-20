using System;
using System.Threading.Tasks;
using Vctoon.Libraries;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Libraries;

public class TagRepositoryTests : VctoonEntityFrameworkCoreTestBase
{
    private readonly ITagRepository _tagRepository;

    public TagRepositoryTests()
    {
        _tagRepository = GetRequiredService<ITagRepository>();
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
