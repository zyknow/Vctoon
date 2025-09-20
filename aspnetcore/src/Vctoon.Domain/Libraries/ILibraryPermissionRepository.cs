using System.Threading;

namespace Vctoon.Libraries;

public interface ILibraryPermissionRepository : IRepository<LibraryPermission>
{
    Task<LibraryPermission?> FindAsync(Guid libraryId, Guid userId, CancellationToken cancellationToken = default);
}