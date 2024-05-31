namespace Vctoon.Identities;

public class IdentityUserLibraryPermissionGrant : AuditedEntity<Guid>
{
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; }
    public List<LibraryPermission> LibraryPermissions { get; set; } = new();
}