namespace Vctoon.Libraries;

public static class LibraryEfCoreQueryableExtensions
{
    public static IQueryable<Library> IncludeDetails(this IQueryable<Library> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable
                .Include(x => x.Paths)
            // .Include(x => x.xxx) // TODO: AbpHelper generated
            ;
    }
}