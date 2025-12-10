using Vctoon.Helper;
using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Mediums;

public partial class MediumAppService
{
#if !DEBUG
    [Authorize]
#endif
    public async Task<RemoteStreamContent> GetComicImageAsync(Guid comicImageId, int? maxWidth = null)
    {
        if (comicImageId == Guid.Empty)
        {
            throw new UserFriendlyException(L["ImageFileIdIsEmpty"]);
        }

        var query = (await comicImageRepository.GetQueryableAsync())
            .Where(x => x.Id == comicImageId)
            .Select(x => new
            {
                x.Id,
                x.Path,
                x.Extension,
                x.ArchiveInfoPathId,
                x.LibraryId,
                x.LibraryPathId
            });
        var imageFile = await AsyncExecuter.FirstOrDefaultAsync(query);

        if (imageFile == null)
        {
            throw new UserFriendlyException(L["ImageFileNotFound"]);
        }
        //TODO: 非部署情况下img标签不会携带cookie，无法鉴权
#if !DEBUG
        await CheckCurrentUserLibraryPermissionAsync(imageFile.LibraryId, x => x.CanView);
#endif

        var contentType = ConverterHelper.MapToRemoteContentType(imageFile.Extension);

        if (imageFile.LibraryPathId is not null)
        {
            var steam = await imageProvider.GetImageStreamAsync(imageFile.Path, maxWidth);
            return new RemoteStreamContent(steam, contentType: contentType);
        }

        var archiveInfo =
            await archiveInfoRepository.FirstOrDefaultAsync(x => x.Paths.Any(p => p.Id == imageFile.ArchiveInfoPathId));

        if (archiveInfo == null)
        {
            throw new UserFriendlyException(L["ArchiveInfoNotFound"]);
        }

        {
            var steam = await imageProvider.GetImageStreamFromArchiveAsync(archiveInfo.Path, imageFile.Path, maxWidth);
            return new RemoteStreamContent(steam, contentType: contentType);
        }
    }

    [Authorize(VctoonPermissions.Comic.Default)]
    public async Task<List<ComicImageDto>> GetComicImageListAsync(Guid mediumId)
    {
        var query = await comicImageRepository.GetQueryableAsync();

        var libraryIds =
            await AsyncExecuter.ToListAsync(query.Where(x => x.MediumId == mediumId).Select(x => x.LibraryId).Distinct());
        foreach (var libraryId in libraryIds)
        {
            await CheckCurrentUserLibraryPermissionAsync(libraryId, x => x.CanView);
        }

        var images = await AsyncExecuter.ToListAsync(query.Where(x => x.MediumId == mediumId).OrderBy(x => x.Name));

        var imageFileDtos = ObjectMapper.Map<List<ComicImage>, List<ComicImageDto>>(
            images);

        return imageFileDtos;
    }

    [Authorize(VctoonPermissions.ImageFile.Delete)]
    public async Task DeleteComicImageAsync(Guid comicImageId, bool deleteFile)
    {
        var file = deleteFile ? await comicImageRepository.GetAsync(comicImageId) : null;

        await comicImageRepository.DeleteAsync(comicImageId, true);

        if (deleteFile)
        {
            if (file != null && File.Exists(file.Path))
            {
                File.Delete(file.Path);
            }
        }
    }

}
