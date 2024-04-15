using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class FavoriteRepository : EfCoreRepository<VctoonDbContext, Favorite, Guid>, IFavoriteRepository
{
    public FavoriteRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Favorite>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}