using Volo.Abp.Domain.Repositories;

namespace Vctoon.Comics;

public interface IComicRepository : IRepository<Comic, Guid>
{
}