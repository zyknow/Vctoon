using Vctoon.Samples;
using Xunit;

namespace Vctoon.EntityFrameworkCore.Domains;

[Collection(VctoonTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<VctoonEntityFrameworkCoreTestModule>
{

}
