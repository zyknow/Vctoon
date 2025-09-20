using Xunit;

namespace Vctoon.EntityFrameworkCore;

[CollectionDefinition(VctoonTestConsts.CollectionDefinitionName)]
public class VctoonEntityFrameworkCoreCollection : ICollectionFixture<VctoonEntityFrameworkCoreFixture>
{

}
