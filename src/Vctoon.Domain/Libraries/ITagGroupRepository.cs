using System;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface ITagGroupRepository : IRepository<TagGroup, Guid>
{
}
