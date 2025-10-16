using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Vctoon.BlobContainers;
using Volo.Abp.BlobStoring;

namespace Vctoon.Services;

public class CoverSaver(IBlobContainer<CoverContainer> coverContainer) : VctoonService, ITransientDependency
{
    public async Task<string> SaveAsync(Stream fileStream)
    {
        using var image = await Image.LoadAsync(fileStream);
        CropImage(image);
        ResizeImage(image);

        using var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream);
        memoryStream.Position = 0;

        var fileName = $@"{Guid.NewGuid()}.png";

        await coverContainer.SaveAsync(fileName, memoryStream,true);

        return fileName;
    }

    private void CropImage(Image image)
    {
        var targetAspectRatio = 2.0 / 3.0;
        var imageAspectRatio = image.Width / (double)image.Height;

        int targetWidth, targetHeight;

        if (imageAspectRatio > targetAspectRatio)
        {
            targetHeight = image.Height;
            targetWidth = (int)(image.Height * targetAspectRatio);
        }
        else
        {
            targetWidth = image.Width;
            targetHeight = (int)(image.Width / targetAspectRatio);
        }

        var resizeOptions = new ResizeOptions
        {
            Size = new Size(targetWidth, targetHeight),
            Mode = ResizeMode.Crop
        };

        image.Mutate(x => x.Resize(resizeOptions));
    }

    private void ResizeImage(Image image)
    {
        var maxWidth = 640;
        var maxHeight = 480;

        if (image.Width <= maxWidth)
        {
            return;
        }


        var ratioX = maxWidth / (double)image.Width;
        var ratioY = maxHeight / (double)image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        image.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
    }
}