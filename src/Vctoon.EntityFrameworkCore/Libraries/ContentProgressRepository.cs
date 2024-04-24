using Microsoft.EntityFrameworkCore;
using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class ContentProgressRepository : EfCoreRepository<VctoonDbContext, ContentProgress, Guid>,
    IContentProgressRepository
{
    public ContentProgressRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<ContentProgress>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }

    public async Task<ContentProgress?> GetOrDefaultAsync(
        Guid? userId,
        Guid? comicId = null,
        Guid? comicChapterId = null
    )
    {
        var query = await GetQueryableAsync();
        return await query
            .WhereIf(comicId != null, x => x.ComicId == comicId)
            .WhereIf(comicChapterId != null, x => x.ComicChapterId == comicChapterId)
            .WhereIf(userId != null, x => x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ContentProgress>> GetListAsync(
        Guid? userId,
        IEnumerable<Guid>? comicIds = null,
        IEnumerable<Guid>? comicChapterIds = null)
    {
        var query = await GetQueryableAsync();
        comicIds = comicIds?.ToList();
        comicChapterIds = comicChapterIds?.ToList();
        return await query
            .WhereIf(comicIds != null && comicIds.Any(), x => comicIds.Contains(x.ComicId.Value))
            .WhereIf(comicChapterIds != null && comicChapterIds.Any(),
                x => comicChapterIds.Contains(x.ComicChapterId.Value))
            .WhereIf(userId != null, x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<double> GetComicCompletionRateAsync(Guid comicId, Guid userId)
    {
        var db = await GetDbContextAsync();
        var chapterCount = await db.ComicChapters.CountAsync(x => x.ComicId == comicId);

        var query = await GetQueryableAsync();
        var rates = await query
            .Where(x => x.ComicId == comicId)
            .Where(x => x.UserId == userId)
            .Select(x => x.CompletionRate).ToListAsync();

        return rates.Sum() / chapterCount;
    }
}