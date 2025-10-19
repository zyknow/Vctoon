using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Zyknow.Abp.Lucene.Localization;

namespace Zyknow.Abp.Lucene.Permissions;

public class ZyknowLucenePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(ZyknowLucenePermissions.GroupName, L("Permission:ZyknowLucene"));

        var indexing = group.AddPermission(ZyknowLucenePermissions.Indexing.Default,
            L("Permission:ZyknowLucene.Indexing"));
        indexing.AddChild(ZyknowLucenePermissions.Indexing.Build, L("Permission:ZyknowLucene.Indexing.Build"));
        indexing.AddChild(ZyknowLucenePermissions.Indexing.Rebuild, L("Permission:ZyknowLucene.Indexing.Rebuild"));
        indexing.AddChild(ZyknowLucenePermissions.Indexing.Clear, L("Permission:ZyknowLucene.Indexing.Clear"));
        indexing.AddChild(ZyknowLucenePermissions.Indexing.Synchronize,
            L("Permission:ZyknowLucene.Indexing.Synchronize"));

        group.AddPermission(ZyknowLucenePermissions.Search.Default, L("Permission:ZyknowLucene.Search"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ZyknowLuceneResource>(name);
    }
}