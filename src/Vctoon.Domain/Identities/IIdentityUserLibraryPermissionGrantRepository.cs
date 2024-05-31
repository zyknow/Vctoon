using Volo.Abp.Domain.Repositories;

namespace Vctoon.Identities;

public interface IIdentityUserLibraryPermissionGrantRepository : IRepository<IdentityUserLibraryPermissionGrant, Guid>
{
}