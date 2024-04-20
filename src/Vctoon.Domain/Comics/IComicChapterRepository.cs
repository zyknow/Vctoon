using Volo.Abp.Domain.Repositories;

namespace Vctoon.Comics;

public interface IComicChapterRepository : IRepository<ComicChapter, Guid>
{
}