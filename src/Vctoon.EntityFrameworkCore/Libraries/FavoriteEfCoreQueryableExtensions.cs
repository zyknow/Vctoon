using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Vctoon.Libraries;

public static class FavoriteEfCoreQueryableExtensions
{
    public static IQueryable<Favorite> IncludeDetails(this IQueryable<Favorite> queryable, bool include = true)
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