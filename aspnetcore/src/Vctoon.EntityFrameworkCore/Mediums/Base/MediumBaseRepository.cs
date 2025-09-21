namespace Vctoon.Mediums.Base;

public abstract class MediumBaseRepository<TEntity>(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, TEntity, Guid>(dbContextProvider),
        IMediumBaseRepository<TEntity>
    where TEntity : MediumBase
{
    public async Task<IQueryable<TEntity>> WithPageDetailsAsync(Guid? userId, bool include = true)
    {
        var query = await GetQueryableAsync();
        query = query
                .Include(x => x.Processes.FirstOrDefault(p => p.UserId == userId))
            ;

        return query;
    }
}