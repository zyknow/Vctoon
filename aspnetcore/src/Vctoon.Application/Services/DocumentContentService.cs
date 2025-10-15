using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Volo.Abp.DependencyInjection;
using VersOne.Epub;

namespace Vctoon.Services;

public record EpubImageDescriptor(string Path, string? ContentType, int Index);

public interface IDocumentContentService
{
    Task<int> GetPdfPageCountAsync(string pdfPath, CancellationToken cancellationToken = default);

    Task<Stream> RenderPdfPageAsync(
        string pdfPath,
        int pageIndex,
        int? maxImageWidth = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EpubImageDescriptor>> GetEpubImageManifestAsync(
        string epubPath,
        CancellationToken cancellationToken = default);

    Task<Stream> GetEpubImageStreamAsync(
        string epubPath,
        string contentPath,
        int? maxImageWidth = null,
        CancellationToken cancellationToken = default);
}

public class DocumentContentService : IDocumentContentService, ITransientDependency
{
    private const int PdfRenderDpi = 144;
    private static readonly PageDimensions PdfPageDimensions = new(PdfRenderDpi / 72d);
    private static readonly SemaphoreSlim PdfRenderSemaphore = new(1, 1);

    public async Task<int> GetPdfPageCountAsync(string pdfPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(pdfPath);

        var entered = false;
        try
        {
            await PdfRenderSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            entered = true;

            using var docReader = DocLib.Instance.GetDocReader(pdfPath, PdfPageDimensions);
            return docReader.GetPageCount();
        }
        finally
        {
            if (entered)
            {
                PdfRenderSemaphore.Release();
            }
        }
    }

    public async Task<Stream> RenderPdfPageAsync(
        string pdfPath,
        int pageIndex,
        int? maxImageWidth = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(pdfPath);

        byte[] rawBytes = null!;
        int width = 0;
        int height = 0;

        var entered = false;
        try
        {
            await PdfRenderSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            entered = true;

            using var docReader = DocLib.Instance.GetDocReader(pdfPath, PdfPageDimensions);

            if (pageIndex < 0 || pageIndex >= docReader.GetPageCount())
            {
                throw new ArgumentOutOfRangeException(nameof(pageIndex));
            }

            using var pageReader = docReader.GetPageReader(pageIndex);
            width = pageReader.GetPageWidth();
            height = pageReader.GetPageHeight();

            rawBytes = pageReader.GetImage(RenderFlags.RenderAnnotations);
        }
        finally
        {
            if (entered)
            {
                PdfRenderSemaphore.Release();
            }
        }

        if (rawBytes is null || width <= 0 || height <= 0)
        {
            throw new InvalidOperationException("Failed to render PDF page.");
        }

        using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
        return await EncodeImageAsync(image, maxImageWidth, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<EpubImageDescriptor>> GetEpubImageManifestAsync(
        string epubPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(epubPath);

        var book = await EpubReader.OpenBookAsync(epubPath).ConfigureAwait(false);
        using (book)
        {
            var results = new List<EpubImageDescriptor>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var index = 0;

            var readingOrder = await book.GetReadingOrderAsync().ConfigureAwait(false);
            foreach (var spineItem in readingOrder)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var content = await spineItem.ReadContentAsTextAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                XDocument? document;
                try
                {
                    document = XDocument.Parse(content);
                }
                catch
                {
                    continue;
                }

                var imgElements = document.Descendants().Where(x => x.Name.LocalName.Equals("img", StringComparison.OrdinalIgnoreCase));
                foreach (var img in imgElements)
                {
                    var src = img.Attribute("src")?.Value;
                    if (string.IsNullOrWhiteSpace(src))
                    {
                        continue;
                    }

                    var resolvedPath = ResolveEpubPath(spineItem.FilePath, src);

                    if (TryAddImageDescriptor(book, resolvedPath, seen, index, results))
                    {
                        index++;
                    }
                }
            }

            foreach (var image in book.Content.Images.Local)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var canonicalPath = NormalizeEpubPath(image.Key);
                if (seen.Contains(canonicalPath))
                {
                    continue;
                }

                results.Add(new EpubImageDescriptor(canonicalPath, image.ContentMimeType, index++));
                seen.Add(canonicalPath);
            }

            return results.OrderBy(x => x.Index).ToList();
        }
    }

    public async Task<Stream> GetEpubImageStreamAsync(
        string epubPath,
        string contentPath,
        int? maxImageWidth = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(epubPath);
        ArgumentException.ThrowIfNullOrEmpty(contentPath);

        var normalizedPath = NormalizeEpubPath(contentPath);

        var book = await EpubReader.OpenBookAsync(epubPath).ConfigureAwait(false);
        using (book)
        {
            if (!TryGetImageFile(book, normalizedPath, out var imageFile))
            {
                throw new FileNotFoundException($"Image '{contentPath}' not found in epub.");
            }

            await using var imageStream = await imageFile.GetContentStreamAsync().ConfigureAwait(false);
            var buffer = new MemoryStream();
            await imageStream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            buffer.Position = 0;

            return await ResizeIfNeededAsync(buffer, maxImageWidth, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool TryAddImageDescriptor(
        EpubBookRef book,
        string normalizedPath,
        HashSet<string> seen,
        int index,
        ICollection<EpubImageDescriptor> results)
    {
        normalizedPath = NormalizeEpubPath(normalizedPath);

        if (!TryGetImageFile(book, normalizedPath, out var imageFile))
        {
            return false;
        }

        var canonicalPath = NormalizeEpubPath(imageFile.Key);
        if (!seen.Add(canonicalPath))
        {
            return false;
        }

        results.Add(new EpubImageDescriptor(canonicalPath, imageFile.ContentMimeType, index));
        return true;
    }

    private static bool TryGetImageFile(EpubBookRef book, string normalizedPath, out EpubLocalByteContentFileRef imageFile)
    {
        var images = book.Content.Images;

        if (images.TryGetLocalFileRefByKey(normalizedPath, out imageFile))
        {
            return true;
        }

        var trimmed = normalizedPath.TrimStart('/');
        if (!ReferenceEquals(trimmed, normalizedPath) && images.TryGetLocalFileRefByKey(trimmed, out imageFile))
        {
            return true;
        }

        if (images.TryGetLocalFileRefByFilePath(normalizedPath, out imageFile))
        {
            return true;
        }

        if (!ReferenceEquals(trimmed, normalizedPath) && images.TryGetLocalFileRefByFilePath(trimmed, out imageFile))
        {
            return true;
        }

        imageFile = null!;
        return false;
    }

    private static async Task<Stream> ResizeIfNeededAsync(Stream source, int? maxImageWidth, CancellationToken cancellationToken)
    {
        if (maxImageWidth is null)
        {
            source.Position = 0;
            return source;
        }

        if (!source.CanSeek)
        {
            var seekable = new MemoryStream();
            await source.CopyToAsync(seekable, cancellationToken).ConfigureAwait(false);
            seekable.Position = 0;
            source.Dispose();
            source = seekable;
        }

        source.Position = 0;
        var imageInfo = await Image.IdentifyAsync(source, cancellationToken).ConfigureAwait(false);
        if (imageInfo is null || imageInfo.Width <= maxImageWidth.Value)
        {
            source.Position = 0;
            return source;
        }

        source.Position = 0;
        using var image = await Image.LoadAsync(source, cancellationToken).ConfigureAwait(false);

        var scale = maxImageWidth.Value / (double)image.Width;
        var targetHeight = Math.Max(1, (int)Math.Round(image.Height * scale));

        image.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(maxImageWidth.Value, targetHeight),
            Mode = ResizeMode.Max
        }));

        var output = new MemoryStream();
    await image.SaveAsync(output, new PngEncoder(), cancellationToken).ConfigureAwait(false);
        output.Position = 0;
        source.Dispose();
        return output;
    }

    private static async Task<Stream> EncodeImageAsync(Image image, int? maxImageWidth, CancellationToken cancellationToken)
    {
        if (maxImageWidth is not null && image.Width > maxImageWidth.Value)
        {
            var scale = maxImageWidth.Value / (double)image.Width;
            var targetHeight = Math.Max(1, (int)Math.Round(image.Height * scale));

            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(maxImageWidth.Value, targetHeight),
                Mode = ResizeMode.Max
            }));
        }

        var output = new MemoryStream();
    await image.SaveAsync(output, new PngEncoder(), cancellationToken).ConfigureAwait(false);
        output.Position = 0;
        return output;
    }

    private static string ResolveEpubPath(string basePath, string relativePath)
    {
        basePath = NormalizeEpubPath(basePath);
        relativePath = NormalizeEpubPath(relativePath);

        if (string.IsNullOrEmpty(relativePath))
        {
            return relativePath;
        }

        if (relativePath.StartsWith('/'))
        {
            return relativePath.TrimStart('/');
        }

        var baseDir = basePath.Contains('/')
            ? basePath[..basePath.LastIndexOf('/')]
            : string.Empty;

        var combined = string.IsNullOrEmpty(baseDir)
            ? relativePath
            : string.Join('/', baseDir, relativePath);

        var segments = new Stack<string>();

        foreach (var segment in combined.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            switch (segment)
            {
                case ".":
                    continue;
                case "..":
                    if (segments.Count > 0)
                    {
                        segments.Pop();
                    }
                    continue;
            }

            segments.Push(segment);
        }

        return string.Join('/', segments.Reverse());
    }

    private static string NormalizeEpubPath(string path)
    {
        return path.Replace('\\', '/');
    }
}