namespace Vctoon.Libraries;

public static class ContentProgressEfCoreQueryableExtensions
{
    public static IQueryable<ContentProgress> IncludeDetails(this IQueryable<ContentProgress> queryable,
        bool include = true)
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