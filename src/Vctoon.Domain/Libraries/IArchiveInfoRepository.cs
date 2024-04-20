using System;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface IArchiveInfoRepository : IRepository<ArchiveInfo, Guid>
{
}
