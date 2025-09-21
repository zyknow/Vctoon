namespace Vctoon.Libraries;

public class ArchiveInfoPathRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, ArchiveInfoPath, Guid>(dbContextProvider),
        IArchiveInfoPathRepository
{
    public override async Task<IQueryable<ArchiveInfoPath>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}