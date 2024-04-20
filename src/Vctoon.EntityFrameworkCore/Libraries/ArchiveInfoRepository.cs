using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class ArchiveInfoRepository : EfCoreRepository<VctoonDbContext, ArchiveInfo, Guid>, IArchiveInfoRepository
{
    public ArchiveInfoRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<ArchiveInfo>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}