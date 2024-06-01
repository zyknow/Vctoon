using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public interface ILibraryPermissionRepository : IRepository<LibraryPermission, Guid>
{
}