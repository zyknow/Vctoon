using System;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface ILibraryRepository : IRepository<Library, Guid>
{
}