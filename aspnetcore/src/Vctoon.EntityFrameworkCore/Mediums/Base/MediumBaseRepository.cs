namespace Vctoon.Mediums.Base;

public abstract class MediumBaseRepository<TEntity>(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : EfCoreRepository<VctoonDbContext, TEntity, Guid>(dbContextProvider),
        IMediumBaseRepository<TEntity>
    where TEntity : MediumBase;