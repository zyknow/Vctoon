namespace Vctoon.Libraries;

public class ArtistRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, Artist, Guid>(dbContextProvider), IArtistRepository
{
    public override async Task<IQueryable<Artist>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}