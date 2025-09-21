using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public class ComicRepository : MediumBaseRepository<Comic>, IComicRepository
{
    public ComicRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Comic>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}