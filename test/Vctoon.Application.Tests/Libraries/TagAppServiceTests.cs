namespace Vctoon.Libraries;

public class TagAppServiceTests : VctoonApplicationTestBase
{
    private readonly ITagAppService _tagAppService;

    public TagAppServiceTests()
    {
        _tagAppService = GetRequiredService<ITagAppService>();
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