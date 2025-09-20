using Vctoon.Mediums;

namespace Vctoon.Handlers;

public interface IMediumScanHandler : ITransientDependency
{
    public Task ScanAsync(LibraryPath libraryPath, MediumType mediaType);
}