namespace Zyknow.Abp.Lucene.Permissions;

public static class ZyknowLucenePermissions
{
    public const string GroupName = "ZyknowLucene";

    public static class Indexing
    {
        public const string Default = GroupName + ".Indexing";
        public const string Build = Default + ".Build";
        public const string Rebuild = Default + ".Rebuild";
        public const string Clear = Default + ".Clear";
        public const string Synchronize = Default + ".Synchronize";
    }

    public static class Search
    {
        public const string Default = GroupName + ".Search";
    }
}