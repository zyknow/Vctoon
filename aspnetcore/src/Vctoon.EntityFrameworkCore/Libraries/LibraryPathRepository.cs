namespace Vctoon.Libraries;

public class LibraryPathRepository : EfCoreRepository<VctoonDbContext, LibraryPath, Guid>, ILibraryPathRepository
{
    public LibraryPathRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<LibraryPath>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}