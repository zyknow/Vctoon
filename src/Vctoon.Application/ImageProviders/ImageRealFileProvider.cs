using SharpCompress.Archives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Vctoon.ImageProviders;

public class ImageRealFileProvider : IImageProvider, ITransientDependency
{
    public async Task<Stream> GetImageStreamAsync(string path, int? maxImageWidth = null)
    {
        if (path.IsNullOrWhiteSpace())
        {
            throw new BusinessException("path is empty");
        }

        var stream = File.OpenRead(path);

        return await ResizeImageAsync(stream, maxImageWidth);
    }

    public async Task<Stream> GetImageStreamFromArchiveAsync(string archivePath, string imagePath,
        int? maxImageWidth = null)
    {
        if (archivePath.IsNullOrWhiteSpace())
        {
            throw new BusinessException("archivePath is empty");
        }

        if (imagePath.IsNullOrWhiteSpace())
        {
            throw new BusinessException("imagePath is empty");
        }

        await using var archiveSteam = File.OpenRead(archivePath);
        using var archive = ArchiveFactory.Open(archiveSteam);


        foreach (var entry in archive.Entries)
        {
            if (!entry.IsDirectory && entry.Key.Equals(imagePath, StringComparison.OrdinalIgnoreCase))
            {
                await using var entryStream = entry.OpenEntryStream();
                var memoryStream = new MemoryStream();
                await entryStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return await ResizeImageAsync(memoryStream, maxImageWidth);
            }
        }

        throw new BusinessException("Image path not found in archive.");
    }

    private async Task<Stream> ResizeImageAsync(Stream stream, int? maxImageWidth = null)
    {
        if (!maxImageWidth.HasValue)
        {
            return stream;
        }

        var image = await Image.LoadAsync(stream);
        stream.Position = 0;

        if (image.Width <= maxImageWidth.Value)
        {
            return stream;
        }

        var scaleFactor = maxImageWidth.Value / (float) image.Width;

        var resizedImage = image.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxImageWidth.Value, (int) (image.Height * scaleFactor))
        }));

        var resizedStream = new MemoryStream();
        await resizedImage.SaveAsJpegAsync(resizedStream);
        resizedStream.Position = 0;
        return resizedStream;
    }
}