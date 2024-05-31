using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Identities;

public class IdentityUserLibraryPermissionGrantRepository :
    EfCoreRepository<VctoonDbContext, IdentityUserLibraryPermissionGrant, Guid>, IIdentityUserLibraryPermissionGrantRepository
{
    public IdentityUserLibraryPermissionGrantRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(
        dbContextProvider)
    {
    }
    
    public override async Task<IQueryable<IdentityUserLibraryPermissionGrant>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}