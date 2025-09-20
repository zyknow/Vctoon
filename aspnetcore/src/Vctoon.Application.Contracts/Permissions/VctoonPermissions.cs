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

    public class ImageFile
    {
        public const string Default = GroupName + ".ImageFile";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public class Tag
    {
        public const string Default = GroupName + ".Tag";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class MediumInfo
    {
        public const string Default = GroupName + ".MediumInfo";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class Video
    {
        public const string Default = GroupName + ".Video";
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

    public class Artist
    {
        public const string Default = GroupName + ".Artist";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
}