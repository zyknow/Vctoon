using Vctoon.Samples;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Applications;

[Collection(VctoonTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<VctoonEntityFrameworkCoreTestModule>
{

}
