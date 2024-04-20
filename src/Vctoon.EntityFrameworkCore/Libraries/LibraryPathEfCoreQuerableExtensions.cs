namespace Vctoon.Libraries;

public static class LibraryPathEfCoreQueryableExtensions
{
    public static IQueryable<LibraryPath> IncludeDetails(this IQueryable<LibraryPath> queryable, bool include = true)
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