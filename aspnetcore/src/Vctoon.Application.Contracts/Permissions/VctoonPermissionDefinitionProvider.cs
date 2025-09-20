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

        var imageFilePermission =
            myGroup.AddPermission(VctoonPermissions.ImageFile.Default, L("Permission:ImageFile"));
        imageFilePermission.AddChild(VctoonPermissions.ImageFile.Update, L("Permission:Update"));
        imageFilePermission.AddChild(VctoonPermissions.ImageFile.Delete, L("Permission:Delete"));


        var tagPermission = myGroup.AddPermission(VctoonPermissions.Tag.Default, L("Permission:Tag"));
        tagPermission.AddChild(VctoonPermissions.Tag.Create, L("Permission:Create"));
        tagPermission.AddChild(VctoonPermissions.Tag.Update, L("Permission:Update"));
        tagPermission.AddChild(VctoonPermissions.Tag.Delete, L("Permission:Delete"));

        var mediumInfoPermission =
            myGroup.AddPermission(VctoonPermissions.MediumInfo.Default, L("Permission:MediumInfo"));
        mediumInfoPermission.AddChild(VctoonPermissions.MediumInfo.Create, L("Permission:Create"));
        mediumInfoPermission.AddChild(VctoonPermissions.MediumInfo.Update, L("Permission:Update"));
        mediumInfoPermission.AddChild(VctoonPermissions.MediumInfo.Delete, L("Permission:Delete"));

        var videoPermission = myGroup.AddPermission(VctoonPermissions.Video.Default, L("Permission:Video"));
        videoPermission.AddChild(VctoonPermissions.Video.Create, L("Permission:Create"));
        videoPermission.AddChild(VctoonPermissions.Video.Update, L("Permission:Update"));
        videoPermission.AddChild(VctoonPermissions.Video.Delete, L("Permission:Delete"));

        var comicPermission = myGroup.AddPermission(VctoonPermissions.Comic.Default, L("Permission:Comic"));
        comicPermission.AddChild(VctoonPermissions.Comic.Create, L("Permission:Create"));
        comicPermission.AddChild(VctoonPermissions.Comic.Update, L("Permission:Update"));
        comicPermission.AddChild(VctoonPermissions.Comic.Delete, L("Permission:Delete"));

        var artistPermission = myGroup.AddPermission(VctoonPermissions.Artist.Default, L("Permission:Artist"));
        artistPermission.AddChild(VctoonPermissions.Artist.Create, L("Permission:Create"));
        artistPermission.AddChild(VctoonPermissions.Artist.Update, L("Permission:Update"));
        artistPermission.AddChild(VctoonPermissions.Artist.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<VctoonResource>(name);
    }
}