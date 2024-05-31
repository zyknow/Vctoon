namespace Vctoon.Identities;

public class IdentityUserLibraryPermissionGrant : AuditedEntity<Guid>
{
    protected IdentityUserLibraryPermissionGrant()
    {
    }
    
    public IdentityUserLibraryPermissionGrant(
        Guid id,
        Guid userId,
        bool isAdmin,
        List<LibraryPermission> libraryPermissions
    ) : base(id)
    {
        UserId = userId;
        IsAdmin = isAdmin;
        LibraryPermissions = libraryPermissions;
    }
    
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; }
    public List<LibraryPermission> LibraryPermissions { get; set; } = new();
}