using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace Vctoon.Mediums.Base;

public abstract class MediumBaseRepository<TEntity>(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, TEntity, Guid>(dbContextProvider),
        IMediumBaseRepository<TEntity>
    where TEntity : MediumBase
{
    public async Task AdditionReadCountAsync(Guid id)
    {
        var db = await GetDbSetAsync(); // DbSet<YourEntity>
        // UPDATE YourTable SET ReadCount = ReadCount + 1 WHERE Id = @id
        var rows = await db.Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ReadCount, x => x.ReadCount + 1));

        if (rows == 0)
        {
            throw new EntityNotFoundException(typeof(TEntity), id);
        }
    }

    public async Task AdditionReadCountAsync(List<Guid> ids)
    {
        if (ids.IsNullOrEmpty())
        {
            return;
        }

        var db = await GetDbSetAsync(); // DbSet<YourEntity>
        // UPDATE YourTable SET ReadCount = ReadCount + 1 WHERE Id IN (@ids)
        await db.Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ReadCount, x => x.ReadCount + 1));
    }

    public async Task<IQueryable<TEntity>> WithUserPageDetailsAsync(Guid? userId, bool include = true)
    {
        var query = await GetQueryableAsync();

        if (!include)
            return query;

        if (userId != null)
        {
            query = query
                    .Include(x => x.Processes.Where(p => p.UserId == userId).Take(1))
                ;
        }


        return query;
    }

    public async Task<IQueryable<TEntity>> WithUserDetailsAsync(Guid? userId, bool include = true)
    {
        var query = await WithUserPageDetailsAsync(userId, include);
        if (!include)
        {
            return query;
        }

        query = query
            .Include(x => x.Tags)
            .Include(x => x.Artists);

        return query;
    }
}