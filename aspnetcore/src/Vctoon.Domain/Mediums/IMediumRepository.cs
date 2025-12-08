namespace Vctoon.Mediums;

public interface IMediumRepository : IRepository<Medium, Guid>
{
    Task AdditionReadCountAsync(Guid id);
    Task AdditionReadCountAsync(List<Guid> ids);
    Task<IQueryable<Medium>> WithUserDetailsAsync(Guid? userId, bool include = true);
    Task<IQueryable<Medium>> WithUserPageDetailsAsync(Guid? userId, bool include = true);
}
