namespace Vctoon.Hubs;

public class HubEventConst
{
    public class Library
    {
        public const string OnScanning = nameof(OnScanning);
        public const string OnScanned = nameof(OnScanned);
    }

    public class DataChangedHub
    {
        public const string OnDeleted = nameof(OnDeleted);
        public const string OnUpdated = nameof(OnUpdated);
        public const string OnCreated = nameof(OnCreated);
    }
}