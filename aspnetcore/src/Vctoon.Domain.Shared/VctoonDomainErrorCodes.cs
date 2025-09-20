namespace Vctoon;

public static class VctoonDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    internal const string VctoonPrefix = "Vctoon:";
    internal const string LibraryPrefix = "Library:";
    public const string DataNotFound = VctoonPrefix + nameof(DataNotFound);


    public const string NameIsAlreadyExists = VctoonPrefix + nameof(NameIsAlreadyExists);

    public const string AdminCanNotChangePermission = VctoonPrefix + nameof(AdminCanNotChangePermission);
}