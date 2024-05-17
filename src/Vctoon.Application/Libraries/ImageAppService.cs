using Vctoon.BlobContainers;
using Vctoon.ImageProviders;
using Volo.Abp;
using Volo.Abp.BlobStoring;

namespace Vctoon.Libraries;

public class ImageAppService(
    IBlobContainer<CoverContainer> coverContainer,
    IImageFileRepository imageFileRepository,
    IImageProvider imageProvider,
    IArchiveInfoRepository archiveInfoRepository)
    : VctoonAppService, IImageAppService
{
    public async Task<Stream> GetCoverAsync(string coverPath)
    {
        await CheckGetCoverPermissionAsync();
        
        if (coverPath.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("Cover path is empty");
        }
        
        var stream = await coverContainer.GetAsync(coverPath);
        return stream;
    }
    
    public async Task<Stream> GetImageAsync(Guid imageFileId, int? maxWidth = null)
    {
        await CheckGetImagePermissionAsync();
        
        if (imageFileId == Guid.Empty)
        {
            throw new UserFriendlyException("Image file id is empty");
        }
        
        var imageFile = await imageFileRepository.GetAsync(imageFileId);
        
        if (imageFile == null)
        {
            throw new UserFriendlyException("Image file not found");
        }
        
        if (imageFile.LibraryPathId is not null)
        {
            return await imageProvider.GetImageStreamAsync(imageFile.Path, maxWidth);
        }
        
        var archiveInfoQuery =
            await archiveInfoRepository.WithDetailsAsync(x =>
                x.Paths);
        
        var archiveInfo = await AsyncExecuter.FirstOrDefaultAsync(archiveInfoQuery.Where(x =>
            x.Paths.First(p => p.Id == imageFile.LibraryPathId).Id == imageFile.LibraryPathId));
        
        if (archiveInfo == null)
        {
            throw new UserFriendlyException("Archive info not found");
        }
        
        return await imageProvider.GetImageStreamFromArchiveAsync(archiveInfo.Path, imageFile.Path, maxWidth);
    }
    
    private async Task CheckGetImagePermissionAsync()
    {
    }
    
    // check permission
    private async Task CheckGetCoverPermissionAsync()
    {
    }
}