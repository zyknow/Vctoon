namespace Vctoon.Mediums;

public static class VideoEfCoreQueryableExtensions
{
    public static IQueryable<Video> IncludeDetails(this IQueryable<Video> queryable, bool include = true)
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