using Vctoon.Localization.Vctoon;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Vctoon.Permissions;

public class VctoonPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(VctoonPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(VctoonPermissions.MyPermission1, L("Permission:MyPermission1"));
        
        var libraryPermission = myGroup.AddPermission(VctoonPermissions.Library.Default, L("Permission:Library"));
        libraryPermission.AddChild(VctoonPermissions.Library.Create, L("Permission:Create"));
        libraryPermission.AddChild(VctoonPermissions.Library.Update, L("Permission:Update"));
        libraryPermission.AddChild(VctoonPermissions.Library.Delete, L("Permission:Delete"));
        
        var comicPermission = myGroup.AddPermission(VctoonPermissions.Comic.Default, L("Permission:Comic"));
        comicPermission.AddChild(VctoonPermissions.Comic.Create, L("Permission:Create"));
        comicPermission.AddChild(VctoonPermissions.Comic.Update, L("Permission:Update"));
        comicPermission.AddChild(VctoonPermissions.Comic.Delete, L("Permission:Delete"));
        
        var tagPermission = myGroup.AddPermission(VctoonPermissions.Tag.Default, L("Permission:Tag"));
        tagPermission.AddChild(VctoonPermissions.Tag.Create, L("Permission:Create"));
        tagPermission.AddChild(VctoonPermissions.Tag.Update, L("Permission:Update"));
        tagPermission.AddChild(VctoonPermissions.Tag.Delete, L("Permission:Delete"));
        
        
        PermissionDefinition identityUserLibraryPermissionGrantPermission = myGroup.AddPermission(
            VctoonPermissions.IdentityUserLibraryPermissionGrant.Default, L("Permission:IdentityUserLibraryPermissionGrant"));
        identityUserLibraryPermissionGrantPermission.AddChild(VctoonPermissions.IdentityUserLibraryPermissionGrant.Create,
            L("Permission:Create"));
        identityUserLibraryPermissionGrantPermission.AddChild(VctoonPermissions.IdentityUserLibraryPermissionGrant.Update,
            L("Permission:Update"));
        identityUserLibraryPermissionGrantPermission.AddChild(VctoonPermissions.IdentityUserLibraryPermissionGrant.Delete,
            L("Permission:Delete"));
        
        PermissionDefinition identityUserExtraPermission =
            myGroup.AddPermission(VctoonPermissions.IdentityUserExtra.Default, L("Permission:IdentityUserExtra"));
        identityUserExtraPermission.AddChild(VctoonPermissions.IdentityUserExtra.Create, L("Permission:Create"));
        identityUserExtraPermission.AddChild(VctoonPermissions.IdentityUserExtra.Update, L("Permission:Update"));
        identityUserExtraPermission.AddChild(VctoonPermissions.IdentityUserExtra.Delete, L("Permission:Delete"));
    }
    
    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<VctoonResource>(name);
    }
}