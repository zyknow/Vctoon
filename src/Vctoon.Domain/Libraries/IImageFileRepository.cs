using System;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface IImageFileRepository : IRepository<ImageFile, Guid>
{
}
