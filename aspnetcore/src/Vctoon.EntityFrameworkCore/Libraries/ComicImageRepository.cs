namespace Vctoon.Libraries;

public class ComicImageRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, ComicImage, Guid>(dbContextProvider), IComicImageRepository
{
    public override async Task<IQueryable<ComicImage>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}