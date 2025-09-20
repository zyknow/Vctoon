namespace Vctoon.Libraries;

public class LibraryRepository : EfCoreRepository<VctoonDbContext, Library, Guid>, ILibraryRepository
{
    public LibraryRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Library>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}