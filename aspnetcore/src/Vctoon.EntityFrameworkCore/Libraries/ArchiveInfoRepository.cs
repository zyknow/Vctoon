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