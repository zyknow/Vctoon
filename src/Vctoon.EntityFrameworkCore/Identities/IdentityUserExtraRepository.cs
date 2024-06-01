using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Identities;

public class IdentityUserExtraRepository : EfCoreRepository<VctoonDbContext, IdentityUserExtra, Guid>,
    IIdentityUserExtraRepository
{
    public IdentityUserExtraRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
    
    public override async Task<IQueryable<IdentityUserExtra>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}