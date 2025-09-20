namespace Vctoon.Libraries;

public static class ArchiveInfoEfCoreQueryableExtensions
{
    public static IQueryable<ArchiveInfo> IncludeDetails(this IQueryable<ArchiveInfo> queryable, bool include = true)
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