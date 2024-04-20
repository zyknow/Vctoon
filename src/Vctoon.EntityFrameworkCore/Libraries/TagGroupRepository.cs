using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class TagGroupRepository : EfCoreRepository<VctoonDbContext, TagGroup, Guid>, ITagGroupRepository
{
    public TagGroupRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<TagGroup>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}