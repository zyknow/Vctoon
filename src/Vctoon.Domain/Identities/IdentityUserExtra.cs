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
        IdentityUser user
    ) : base(id)
    {
        UserId = userId;
        User = user;
    }

    public Guid UserId { get; set; }
    public IdentityUser User { get; set; }
}