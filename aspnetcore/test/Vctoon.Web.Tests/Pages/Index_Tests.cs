using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Vctoon.Pages;

[Collection(VctoonTestConsts.CollectionDefinitionName)]
public class Index_Tests : VctoonWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}