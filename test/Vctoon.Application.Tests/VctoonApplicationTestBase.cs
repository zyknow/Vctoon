using Volo.Abp.Modularity;

namespace Vctoon;

public abstract class VctoonApplicationTestBase<TStartupModule> : VctoonTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
