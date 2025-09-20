namespace Vctoon.Libraries;

public static class TagEfCoreQueryableExtensions
{
    public static IQueryable<Tag> IncludeDetails(this IQueryable<Tag> queryable, bool include = true)
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