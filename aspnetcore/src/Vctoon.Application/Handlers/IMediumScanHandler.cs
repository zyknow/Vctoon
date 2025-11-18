using Vctoon.Mediums;

namespace Vctoon.Handlers;

public interface IMediumScanHandler : ITransientDependency
{
    public Task<ScanResult?> ScanAsync(LibraryPath libraryPath, MediumType mediaType);
}

public sealed record ScanResult(int Deleted, int Updated, int Added);