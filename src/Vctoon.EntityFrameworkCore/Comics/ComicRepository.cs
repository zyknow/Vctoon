using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Comics;

public class ComicRepository : EfCoreRepository<VctoonDbContext, Comic, Guid>, IComicRepository
{
    public ComicRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Comic>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}