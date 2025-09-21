namespace Vctoon.Libraries;

public class ArchiveInfoRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, ArchiveInfo, Guid>(dbContextProvider), IArchiveInfoRepository
{
    public override async Task<IQueryable<ArchiveInfo>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}