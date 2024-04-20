using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class ArchiveInfoPathRepository : EfCoreRepository<VctoonDbContext, ArchiveInfoPath, Guid>, IArchiveInfoPathRepository
{
    public ArchiveInfoPathRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<ArchiveInfoPath>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}