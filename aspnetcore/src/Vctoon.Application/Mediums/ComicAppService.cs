using System.Linq.Expressions;
using Vctoon.Helper;
using Vctoon.Identities;
using Vctoon.ImageProviders;
using Vctoon.Libraries.Dtos;
using Vctoon.Mediums.Base;
using Vctoon.Mediums.Dtos;
using Vctoon.Permissions;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Mediums;

public class ComicAppService(
    IComicRepository repository,
    IComicImageRepository comicImageRepository,
    IImageProvider imageProvider,
    IArchiveInfoRepository archiveInfoRepository)
    : MediumBaseAppService<Comic, ComicDto, ComicGetListOutputDto, ComicGetListInput, ComicCreateUpdateDto,
            ComicCreateUpdateDto>(repository),
        IComicAppService
{
    protected override string? GetPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string? GetListPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string? CreatePolicyName { get; set; } = VctoonPermissions.Comic.Create;
    protected override string? UpdatePolicyName { get; set; } = VctoonPermissions.Comic.Update;
    protected override string? DeletePolicyName { get; set; } = VctoonPermissions.Comic.Delete;


    // 原为布尔谓词，现改为属性选择器，交由基类统一转换
    protected override LambdaExpression ProcessKeySelector =>
        (Expression<Func<IdentityUserReadingProcess, Guid?>>)(p => p.ComicId);

    [RemoteService(false)]
    public override Task<ComicDto> CreateAsync(ComicCreateUpdateDto input)
    {
        throw new UserFriendlyException("Not supported");
    }

#if !DEBUG
    [Authorize]
#endif
    public async Task<RemoteStreamContent> GetComicImageAsync(Guid comicImageId, int? maxWidth = null)
    {
        if (comicImageId == Guid.Empty)
        {
            throw new UserFriendlyException("Image file id is empty");
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
            throw new UserFriendlyException("Image file not found");
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
            throw new UserFriendlyException("Archive info not found");
        }

        {
            var steam = await imageProvider.GetImageStreamFromArchiveAsync(archiveInfo.Path, imageFile.Path, maxWidth);
            return new RemoteStreamContent(steam, contentType: contentType);
        }
    }

    [Authorize(VctoonPermissions.Comic.Default)]
    public async Task<List<ComicImageDto>> GetListByComicIdAsync(Guid comicId)
    {
        var query = await comicImageRepository.GetQueryableAsync();

        var libraryIds =
            await AsyncExecuter.ToListAsync(query.Where(x => x.ComicId == comicId).Select(x => x.LibraryId).Distinct());
        foreach (var libraryId in libraryIds)
        {
            await CheckCurrentUserLibraryPermissionAsync(libraryId, x => x.CanView);
        }

        var images = await AsyncExecuter.ToListAsync(query.Where(x => x.ComicId == comicId).OrderBy(x => x.Name));

        var imageFileDtos = ObjectMapper.Map<List<ComicImage>, List<ComicImageDto>>(
            images);

        return imageFileDtos;
    }

    [Authorize(VctoonPermissions.ImageFile.Delete)]
    public async Task DeleteComicImageAsync(Guid comicImageId, bool deleteFile)
    {
        var file = deleteFile ? await comicImageRepository.GetAsync(comicImageId) : null;

        await repository.DeleteAsync(comicImageId, deleteFile);

        if (deleteFile)
        {
            if (file != null && File.Exists(file.Path))
            {
                File.Delete(file.Path);
            }
        }
    }
}