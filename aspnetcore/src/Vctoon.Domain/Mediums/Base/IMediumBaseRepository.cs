namespace Vctoon.Mediums.Base;

public interface IMediumBaseRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : MediumBase
{
}