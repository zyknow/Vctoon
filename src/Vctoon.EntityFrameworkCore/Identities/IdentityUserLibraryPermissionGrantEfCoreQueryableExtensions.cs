namespace Vctoon.Identities;

public static class IdentityUserLibraryPermissionGrantEfCoreQueryableExtensions
{
    public static IQueryable<IdentityUserLibraryPermissionGrant> IncludeDetails(
        this IQueryable<IdentityUserLibraryPermissionGrant> queryable, bool include = true)
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