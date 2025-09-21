namespace Vctoon.Libraries;

public class TagRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, Tag, Guid>(dbContextProvider), ITagRepository
{
    public override async Task<IQueryable<Tag>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}