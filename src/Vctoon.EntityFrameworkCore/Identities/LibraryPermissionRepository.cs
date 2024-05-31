using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Identities;

public class LibraryPermissionRepository : EfCoreRepository<VctoonDbContext, LibraryPermission, Guid>,
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