namespace Vctoon.Libraries;

public class TagRepository : EfCoreRepository<VctoonDbContext, Tag, Guid>, ITagRepository
{
    public TagRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Tag>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}