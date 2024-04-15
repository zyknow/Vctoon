using Volo.Abp.Modularity;

namespace Vctoon;

/* Inherit from this class for your domain layer tests. */
public abstract class VctoonDomainTestBase<TStartupModule> : VctoonTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
