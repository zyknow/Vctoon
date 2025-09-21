namespace Vctoon.Mediums.Base;

public abstract class MediumBaseRepository<TEntity>(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, TEntity, Guid>(dbContextProvider),
        IMediumBaseRepository<TEntity>
    where TEntity : MediumBase
{
    public async Task<IQueryable<TEntity>> WithPageDetailsAsync(Guid? userId, bool include = true)
    {
        var query = await GetQueryableAsync();
        query = query.AsNoTracking();
        if (userId != null)
        {
            query = query
                    .Include(x => x.Processes.Where(p => p.UserId == userId).Take(1))
                ;
        }


        return query;
    }
}