using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public class ComicRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : MediumBaseRepository<Comic>(dbContextProvider), IComicRepository
{
    public override async Task<IQueryable<Comic>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}