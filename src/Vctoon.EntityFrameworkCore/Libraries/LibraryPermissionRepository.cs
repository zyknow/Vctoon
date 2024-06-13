using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class LibraryPermissionRepository : EfCoreRepository<VctoonDbContext, LibraryPermission>,
    ILibraryPermissionRepository
{
    public LibraryPermissionRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<LibraryPermission>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}