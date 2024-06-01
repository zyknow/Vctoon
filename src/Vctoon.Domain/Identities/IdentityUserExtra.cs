using Vctoon.Libraries;
using Volo.Abp.Identity;

namespace Vctoon.Identities;

public class IdentityUserExtra : Entity<Guid>
{
    protected IdentityUserExtra()
    {
    }
    
    public IdentityUserExtra(
        Guid id,
        Guid userId,
        IdentityUser user,
        List<LibraryPermission> libraryPermissions
    ) : base(id)
    {
        UserId = userId;
        User = user;
        LibraryPermissions = libraryPermissions;
    }
    
    public Guid UserId { get; set; }
    public IdentityUser User { get; set; }
    public List<LibraryPermission> LibraryPermissions { get; set; } = new();
}