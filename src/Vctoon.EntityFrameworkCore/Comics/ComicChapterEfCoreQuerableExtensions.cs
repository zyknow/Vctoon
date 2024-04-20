using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Vctoon.Comics;

public static class ComicChapterEfCoreQueryableExtensions
{
    public static IQueryable<ComicChapter> IncludeDetails(this IQueryable<ComicChapter> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable
            // .Include(x => x.xxx) // TODO: AbpHelper generated
            ;
    }
}
