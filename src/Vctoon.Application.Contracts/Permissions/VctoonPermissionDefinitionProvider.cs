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

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<VctoonResource>(name);
    }
}