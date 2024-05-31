namespace Vctoon.Permissions;

public static class VctoonPermissions
{
    public const string GroupName = "Vctoon";
    
    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
    
    
    public class Library
    {
        public const string Default = GroupName + ".Library";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
    
    public class Comic
    {
        public const string Default = GroupName + ".Comic";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
    
    public class Tag
    {
        public const string Default = GroupName + ".Tag";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
    
    public class TagGroup
    {
        public const string Default = GroupName + ".TagGroup";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
    
    public class IdentityUserLibraryPermissionGrant
    {
        public const string Default = GroupName + ".IdentityUserLibraryPermissionGrant";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
}