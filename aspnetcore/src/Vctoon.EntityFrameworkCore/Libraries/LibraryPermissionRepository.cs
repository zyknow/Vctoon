using System.Threading;

namespace Vctoon.Libraries;

public class LibraryPermissionRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, LibraryPermission>(dbContextProvider),
        ILibraryPermissionRepository
{
    public override async Task<IQueryable<LibraryPermission>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }

    public async Task<LibraryPermission?> FindAsync(Guid libraryId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.LibraryId == libraryId && x.UserId == userId, cancellationToken);
    }
}