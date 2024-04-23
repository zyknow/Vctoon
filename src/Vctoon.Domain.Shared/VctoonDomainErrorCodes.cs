namespace Vctoon;

public static class VctoonDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    internal const string Prefix = "Vctoon:";
    public const string DataNotFound = Prefix + nameof(DataNotFound);
    public const string NameIsAlreadyExists = Prefix + nameof(NameIsAlreadyExists);
}