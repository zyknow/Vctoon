using Microsoft.AspNetCore.Authorization;
using Vctoon.Extensions;
using Vctoon.Libraries.Dtos;
using Vctoon.Services;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries;

[Authorize]
public class ImageFileAppService(
    IImageFileRepository repository,
    ILibraryPermissionChecker libraryPermissionChecker,
    ILibraryPermissionRepository libraryPermissionRepository) :
    VctoonAbstractKeyReadOnlyAppService<ImageFile, ImageFileDto, Guid, ImageFileGetListInput>(repository,
        libraryPermissionChecker),
    IImageFileAppService
{
    public override async Task<PagedResultDto<ImageFileDto>> GetListAsync(ImageFileGetListInput input)
    {
        PagedResultDto<ImageFileDto> pageRes = await base.GetListAsync(input);
        List<Guid> libraryIds = pageRes.Items.Select(x => x.LibraryId).Distinct().ToList();

        foreach (Guid libraryId in libraryIds)
        {
            await CheckCurrentUserLibraryPermissionAsync(libraryId, x => x.CanView);
        }

        return pageRes;
    }

    [Authorize(VctoonPermissions.ImageFile.Delete)]
    public async Task DeleteAsync(Guid id, bool deleteFile)
    {
        ImageFile file = deleteFile ? await repository.GetAsync(id) : null;

        await repository.DeleteAsync(id);

        if (deleteFile)
        {
            UnitOfWorkManager.Current.OnCompleted(async () =>
            {
                if (File.Exists(file.Path))
                {
                    File.Delete(file.Path);
                }
            });
        }
    }

    protected override async Task<IQueryable<ImageFile>> CreateFilteredQueryAsync(ImageFileGetListInput input)
    {
        if (input.ComicId == null && input.LibraryPathId == null && input.ArchiveInfoPathId == null)
        {
            throw new UserFriendlyException("ComicId or LibraryPathId or ArchiveInfoPathId must be set.");
        }

        IQueryable<ImageFile>? query = await repository.GetQueryableAsync();

        query = query
                .WhereIf(input.ComicId != null, x => x.ComicId == input.ComicId)
                .WhereIf(input.Extension != null, x => x.Extension == input.Extension)
                .WhereIf(input.LibraryPathId != null, x => x.LibraryPathId == input.LibraryPathId)
                .WhereIf(input.Name != null, x => x.Name == input.Name)
                .WhereIf(input.Path != null, x => x.Path == input.Path)
                .WhereIf(input.Size != null, x => x.Size == input.Size)
            ;

        if (!CurrentUser.IsAdmin())
        {
            List<Guid> libraryIds = await AsyncExecuter.ToListAsync((await libraryPermissionRepository.GetQueryableAsync())
                .Where(x => x.UserId == CurrentUser.Id)
                .Select(x => x.LibraryId));

            query = query.Where(x => libraryIds.Contains(x.LibraryId));
        }

        return query;
    }

    public override async Task<ImageFileDto> GetAsync(Guid id)
    {
        ImageFileDto imageFileDto = await base.GetAsync(id);
        await CheckCurrentUserLibraryPermissionAsync(imageFileDto.LibraryId, x => x.CanView);
        return imageFileDto;
    }

    protected override Task<ImageFile> GetEntityByIdAsync(Guid id)
    {
        return repository.GetAsync(id);
    }

    [Authorize(VctoonPermissions.ImageFile.Update)]
    public async Task<ImageFileDto> UpdateAsync(Guid id, ImageFileCreateUpdateDto input)
    {
        ImageFile imageFile = await repository.GetAsync(id);

        imageFile = ObjectMapper.Map(input, imageFile);
        await repository.UpdateAsync(imageFile);

        return ObjectMapper.Map<ImageFile, ImageFileDto>(imageFile);
    }

    protected override IQueryable<ImageFile> ApplyDefaultSorting(IQueryable<ImageFile> query)
    {
        return query.OrderBy(e => e.Name);
    }
}