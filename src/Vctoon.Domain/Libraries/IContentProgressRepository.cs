using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface IContentProgressRepository : IRepository<ContentProgress, Guid>
{
    Task<IQueryable<ContentProgress>> WithDetailsAsync();

    Task<ContentProgress?> GetOrDefaultAsync(
        Guid? userId,
        Guid? comicId = null,
        Guid? comicChapterId = null
    );

    Task<List<ContentProgress>> GetListAsync(
        Guid? userId,
        IEnumerable<Guid>? comicIds = null,
        IEnumerable<Guid>? comicChapterIds = null);

    Task<double> GetComicCompletionRateAsync(Guid comicId, Guid userId);
}