namespace Vctoon.Mediums;

public static class ComicEfCoreQueryableExtensions
{
    public static IQueryable<Comic> IncludeDetails(this IQueryable<Comic> queryable, bool include = true)
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