namespace Vctoon.Identities;

public class IdentityUserReadingProcessRepository(
    IDbContextProvider<VctoonDbContext> dbContextProvider
) : EfCoreRepository<VctoonDbContext, IdentityUserReadingProcess>(dbContextProvider),
    IIdentityUserReadingProcessRepository
{
}