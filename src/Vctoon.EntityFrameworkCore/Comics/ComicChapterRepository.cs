using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Comics;

public class ComicChapterRepository : EfCoreRepository<VctoonDbContext, ComicChapter, Guid>, IComicChapterRepository
{
    public ComicChapterRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<ComicChapter>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}