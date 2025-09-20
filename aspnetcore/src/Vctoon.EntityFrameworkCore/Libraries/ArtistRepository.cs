namespace Vctoon.Libraries;

public class ArtistRepository : EfCoreRepository<VctoonDbContext, Artist, Guid>, IArtistRepository
{
    public ArtistRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Artist>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}