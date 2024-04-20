namespace Vctoon.Libraries;

public static class ArchiveInfoPathEfCoreQueryableExtensions
{
    public static IQueryable<ArchiveInfoPath> IncludeDetails(this IQueryable<ArchiveInfoPath> queryable,
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