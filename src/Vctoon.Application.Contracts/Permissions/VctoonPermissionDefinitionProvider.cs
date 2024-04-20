using Vctoon.Localization;
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

        var comicChapterPermission = myGroup.AddPermission(VctoonPermissions.ComicChapter.Default, L("Permission:ComicChapter"));
        comicChapterPermission.AddChild(VctoonPermissions.ComicChapter.Create, L("Permission:Create"));
        comicChapterPermission.AddChild(VctoonPermissions.ComicChapter.Update, L("Permission:Update"));
        comicChapterPermission.AddChild(VctoonPermissions.ComicChapter.Delete, L("Permission:Delete"));

        var tagPermission = myGroup.AddPermission(VctoonPermissions.Tag.Default, L("Permission:Tag"));
        tagPermission.AddChild(VctoonPermissions.Tag.Create, L("Permission:Create"));
        tagPermission.AddChild(VctoonPermissions.Tag.Update, L("Permission:Update"));
        tagPermission.AddChild(VctoonPermissions.Tag.Delete, L("Permission:Delete"));

        var tagGroupPermission = myGroup.AddPermission(VctoonPermissions.TagGroup.Default, L("Permission:TagGroup"));
        tagGroupPermission.AddChild(VctoonPermissions.TagGroup.Create, L("Permission:Create"));
        tagGroupPermission.AddChild(VctoonPermissions.TagGroup.Update, L("Permission:Update"));
        tagGroupPermission.AddChild(VctoonPermissions.TagGroup.Delete, L("Permission:Delete"));

        var imageFilePermission = myGroup.AddPermission(VctoonPermissions.ImageFile.Default, L("Permission:ImageFile"));
        imageFilePermission.AddChild(VctoonPermissions.ImageFile.Create, L("Permission:Create"));
        imageFilePermission.AddChild(VctoonPermissions.ImageFile.Update, L("Permission:Update"));
        imageFilePermission.AddChild(VctoonPermissions.ImageFile.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<VctoonResource>(name);
    }
}
