namespace Vctoon.Mediums.Base;

public interface IMediumBaseRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : MediumBase
{
    public Task<IQueryable<TEntity>> WithPageDetailsAsync(Guid? userId,
        bool include = true);
}