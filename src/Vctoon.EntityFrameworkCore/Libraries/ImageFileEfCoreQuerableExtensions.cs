namespace Vctoon.Libraries;

public static class ImageFileEfCoreQueryableExtensions
{
    public static IQueryable<ImageFile> IncludeDetails(this IQueryable<ImageFile> queryable, bool include = true)
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