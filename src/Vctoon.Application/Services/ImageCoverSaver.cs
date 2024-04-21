using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Vctoon.BlobContainers;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Services;

public class ImageCoverSaver(IBlobContainer<CoverContainer> coverContainer) : VctoonService, ITransientDependency
{
    public async Task<string> SaveAsync(Stream fileStream)
    {
        using var image = await Image.LoadAsync(fileStream);

        // cover size get from settings
        var width = image.Width > 720 ? 720 : image.Width;
        var height = image.Height > 1280 ? 1280 : image.Height;

        //TODO: Resize image if to height, clip to with and height
        image.Mutate(x => x.Resize(width, height,
            KnownResamplers.Lanczos3));

        using var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream);

        var fileName = $@"{Guid.NewGuid()}.png";

        await coverContainer.SaveAsync(fileName, memoryStream);

        return fileName;
    }
}