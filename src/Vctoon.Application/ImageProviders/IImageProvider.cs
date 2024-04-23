namespace Vctoon.ImageProviders;

public interface IImageProvider
{
    Task<Stream> GetImageStreamAsync(string path, int? maxImageWidth = null);
    Task<Stream> GetImageStreamFromArchiveAsync(string archivePath, string imagePath, int? maxImageWidth = null);
}