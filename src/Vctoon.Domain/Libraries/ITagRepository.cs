using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface ITagRepository : IRepository<Tag, Guid>
{
}