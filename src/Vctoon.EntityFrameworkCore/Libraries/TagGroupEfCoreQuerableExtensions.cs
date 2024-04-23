using Microsoft.EntityFrameworkCore;

namespace Vctoon.Libraries;

public static class TagGroupEfCoreQueryableExtensions
{
    public static IQueryable<TagGroup> IncludeDetails(this IQueryable<TagGroup> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable
                .Include(x => x.Tags)
            // .Include(x => x.xxx) // TODO: AbpHelper generated
            ;
    }
}