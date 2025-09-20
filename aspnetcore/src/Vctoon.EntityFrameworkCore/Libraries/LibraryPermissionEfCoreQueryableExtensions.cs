namespace Vctoon.Libraries;

public static class LibraryPermissionEfCoreQueryableExtensions
{
    public static IQueryable<LibraryPermission> IncludeDetails(this IQueryable<LibraryPermission> queryable,
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