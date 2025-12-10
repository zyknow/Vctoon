using System.Globalization;
using Microsoft.Extensions.Localization;
using SharpCompress.Archives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Vctoon.Localization.Vctoon;
using Volo.Abp;
using Size = SixLabors.ImageSharp.Size;

namespace Vctoon.ImageProviders;

public class ImageRealFileProvider(IDocumentContentService documentContentService, IStringLocalizer<VctoonResource> l)
    : IImageProvider, ITransientDependency
{
    private static readonly HashSet<string> ArchiveExtensions =
        new([".zip", ".rar", ".cbr", ".cbz", ".7z", ".cb7"], StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> PdfExtensions =
        new([".pdf"], StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> EpubExtensions =
        new([".epub"], StringComparer.OrdinalIgnoreCase);

    public async Task<Stream> GetImageStreamAsync(string path, int? maxImageWidth = null)
    {
        if (path.IsNullOrWhiteSpace())
        {
            throw new BusinessException(l["PathIsEmpty"]);
        }

        var stream = File.OpenRead(path);

        return await ResizeImageAsync(stream, maxImageWidth);
    }

    public async Task<Stream> GetImageStreamFromArchiveAsync(string archivePath, string imagePath,
        int? maxImageWidth = null)
    {
        if (archivePath.IsNullOrWhiteSpace())
        {
            throw new BusinessException(l["ArchivePathIsEmpty"]);
        }

        if (imagePath.IsNullOrWhiteSpace())
        {
            throw new BusinessException(l["ImagePathIsEmpty"]);
        }

        var extension = Path.GetExtension(archivePath).ToLowerInvariant();

        if (PdfExtensions.Contains(extension))
        {
            var pageIndex = ParsePdfPageKey(imagePath);
            return await documentContentService.RenderPdfPageAsync(archivePath, pageIndex, maxImageWidth)
                .ConfigureAwait(false);
        }

        if (EpubExtensions.Contains(extension))
        {
            return await documentContentService.GetEpubImageStreamAsync(archivePath, imagePath, maxImageWidth)
                .ConfigureAwait(false);
        }

        if (!ArchiveExtensions.Contains(extension))
        {
            throw new BusinessException(l["UnsupportedArchiveExtension", extension]);
        }

        await using var archiveSteam = File.OpenRead(archivePath);
        using var archive = ArchiveFactory.Open(archiveSteam);


        foreach (var entry in archive.Entries)
        {
            if (!entry.IsDirectory && entry.Key!.Equals(imagePath, StringComparison.OrdinalIgnoreCase))
            {
                await using var entryStream = entry.OpenEntryStream();
                var memoryStream = new MemoryStream();
                await entryStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return await ResizeImageAsync(memoryStream, maxImageWidth);
            }
        }

        throw new BusinessException(l["ImagePathNotFoundInArchive"]);
    }

    private async Task<Stream> ResizeImageAsync(Stream stream, int? maxImageWidth = null)
    {
        if (!maxImageWidth.HasValue)
            return stream;

        // 先读取图片元数据，避免不必要的解码
        var info = await Image.IdentifyAsync(stream);
        if (info == null || info.Width <= maxImageWidth.Value)
        {
            stream.Position = 0;
            return stream;
        }

        stream.Position = 0;
        using var image = await Image.LoadAsync(stream);

        var scaleFactor = maxImageWidth.Value / (float)image.Width;
        var resizedImage = image.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxImageWidth.Value, (int)(image.Height * scaleFactor))
        }));

        var resizedStream = new MemoryStream();
        await resizedImage.SaveAsJpegAsync(resizedStream);
        resizedStream.Position = 0;
        return resizedStream;
    }

    private int ParsePdfPageKey(string key)
    {
        const string prefix = "pdf-page-";

        if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(l["InvalidPdfPageToken", key]);
        }

        var numeric = key.AsSpan(prefix.Length);
        if (!int.TryParse(numeric, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            throw new BusinessException(l["InvalidPdfPageToken", key]);
        }

        return index;
    }
}