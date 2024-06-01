namespace Vctoon.Identities;

public static class IdentityUserExtraEfCoreQueryableExtensions
{
    public static IQueryable<IdentityUserExtra> IncludeDetails(this IQueryable<IdentityUserExtra> queryable, bool include = true)
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