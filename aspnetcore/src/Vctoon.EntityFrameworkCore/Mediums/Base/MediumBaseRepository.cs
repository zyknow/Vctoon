using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public abstract class MediumBaseRepository<TEntity> : EfCoreRepository<VctoonDbContext, TEntity, Guid>,
    IMediumBaseRepository<TEntity> where TEntity : MediumBase
{
    protected MediumBaseRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}