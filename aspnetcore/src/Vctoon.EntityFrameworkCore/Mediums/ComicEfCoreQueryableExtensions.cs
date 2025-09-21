namespace Vctoon.Mediums;

public static class ComicEfCoreQueryableExtensions
{
    public static IQueryable<Comic> IncludeDetails(this IQueryable<Comic> queryable,
        bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        queryable = queryable
            // .Include(x => x.xxx) // TODO: AbpHelper generated
            ;

        return queryable;
    }
}