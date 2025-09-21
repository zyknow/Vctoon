namespace Vctoon.Libraries;

public class LibraryPathRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, LibraryPath, Guid>(dbContextProvider), ILibraryPathRepository
{
    public override async Task<IQueryable<LibraryPath>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}