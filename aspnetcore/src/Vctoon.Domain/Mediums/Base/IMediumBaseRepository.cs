namespace Vctoon.Mediums.Base;

public interface IMediumBaseRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : MediumBase
{
    public Task<IQueryable<TEntity>> WithUserDetailsAsync(Guid? userId,
        bool include = true);

    Task<IQueryable<TEntity>> WithUserPageDetailsAsync(Guid? userId, bool include = true);
    Task AdditionReadCountAsync(Guid id);
    Task AdditionReadCountAsync(List<Guid> ids);
}