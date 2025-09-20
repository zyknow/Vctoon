namespace Vctoon.Libraries;

public class ArchiveInfoPathRepository : EfCoreRepository<VctoonDbContext, ArchiveInfoPath, Guid>,
    IArchiveInfoPathRepository
{
    public ArchiveInfoPathRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<ArchiveInfoPath>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}