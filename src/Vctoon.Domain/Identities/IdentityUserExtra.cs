using Vctoon.Libraries;
using Volo.Abp.Identity;

namespace Vctoon.Identities;

public class IdentityUserExtra : Entity<Guid>
{
    public Guid UserId { get; set; }
    public IdentityUser User { get; set; }
    public List<LibraryPermission> LibraryPermissions { get; set; } = new();
}