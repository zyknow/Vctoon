namespace Vctoon.Libraries;

public class TagGroupAppServiceTests : VctoonApplicationTestBase
{
    private readonly ITagGroupAppService _tagGroupAppService;

    public TagGroupAppServiceTests()
    {
        _tagGroupAppService = GetRequiredService<ITagGroupAppService>();
    }

    /*
    [Fact]
    public async Task Test1()
    {
        // Arrange

        // Act

        // Assert
    }
    */
}