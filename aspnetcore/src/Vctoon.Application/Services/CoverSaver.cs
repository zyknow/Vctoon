using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats; // placeholder color
using Vctoon.BlobContainers;
using Volo.Abp.BlobStoring;
using Microsoft.Extensions.Logging;

namespace Vctoon.Services;

public class CoverSaver(IBlobContainer<CoverContainer> coverContainer) : VctoonService, ITransientDependency
{
    const int MaxWidthOrHeight = 350;
    const double MaxAspectRatio = 1.7;
    const int Quality = 50;

    public async Task<string> SaveAsync(Stream fileStream)
    {
        if (fileStream == null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }
        if (fileStream.CanSeek && fileStream.Position != 0)
        {
            fileStream.Position = 0;
        }
        if (fileStream.CanSeek && fileStream.Length == 0)
        {
            Logger.LogWarning("Cover stream empty. Using placeholder image.");
            return await SavePlaceholderAsync();
        }

        Image? image = null;
        try
        {
            image = await Image.LoadAsync(fileStream); // try decode
        }
        catch (SixLabors.ImageSharp.UnknownImageFormatException ex)
        {
            Logger.LogWarning(ex, "Unknown image format encountered when saving cover. Using placeholder instead.");
            return await SavePlaceholderAsync();
        }

        using (image)
        {
            ResizeImage(image);
            using var memoryStream = new MemoryStream();
            await SaveWebpAsync(image, memoryStream);
            memoryStream.Position = 0;
            var fileName = $"{Guid.NewGuid()}.webp";
            await coverContainer.SaveAsync(fileName, memoryStream, true);
            return fileName;
        }
    }

    private async Task<string> SavePlaceholderAsync()
    {
        using var img = new Image<Rgba32>(MaxWidthOrHeight, MaxWidthOrHeight, new Rgba32(40, 40, 40, 255));
        using var ms = new MemoryStream();
        await SaveWebpAsync(img, ms);
        ms.Position = 0;
        var fileName = $"{Guid.NewGuid()}.webp";
        await coverContainer.SaveAsync(fileName, ms, true);
        return fileName;
    }

    private async Task SaveWebpAsync(Image image, Stream output)
    {
        image.Metadata.ExifProfile = null; // strip metadata
        var encoder = new WebpEncoder
        {
            FileFormat = WebpFileFormatType.Lossy,
            Quality = Quality, // lowered quality for better compression
            EntropyPasses = 1,
            NearLossless = false
        };
        await image.SaveAsWebpAsync(output, encoder);
    }

    private void ResizeImage(Image image)
    {
        // Scale based on the larger dimension so that the largest side does not exceed MaxWidthOrHeight.
        var width = image.Width;
        var height = image.Height;
        var max = Math.Max(width, height);
        if (max > MaxWidthOrHeight)
        {
            var scale = MaxWidthOrHeight / (double)max;
            var newWidth = (int)Math.Round(width * scale);
            var newHeight = (int)Math.Round(height * scale);
            image.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
            width = newWidth;
            height = newHeight;
        }

        // Crop if aspect ratio exceeds MaxAspectRatio (too wide or too tall)
        var aspect = width / (double)height;
        if (aspect > MaxAspectRatio)
        {
            var targetWidth = (int)Math.Round(height * MaxAspectRatio);
            if (targetWidth < width)
            {
                var left = (width - targetWidth) / 2;
                image.Mutate(x => x.Crop(new Rectangle(left, 0, targetWidth, height)));
            }
        }
        else if ((1 / aspect) > MaxAspectRatio)
        {
            var targetHeight = (int)Math.Round(width * MaxAspectRatio);
            if (targetHeight < height)
            {
                var top = (height - targetHeight) / 2;
                image.Mutate(x => x.Crop(new Rectangle(0, top, width, targetHeight)));
            }
        }
    }
}