namespace Vctoon.Libraries;

public class LibraryRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, Library, Guid>(dbContextProvider), ILibraryRepository
{
    public override async Task<IQueryable<Library>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}