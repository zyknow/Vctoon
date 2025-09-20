namespace Vctoon.Libraries;

public static class ComicImageEfCoreQueryableExtensions
{
    public static IQueryable<ComicImage> IncludeDetails(this IQueryable<ComicImage> queryable, bool include = true)
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