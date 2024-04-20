using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.Permissions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;


public class ImageFileAppService : CrudAppService<ImageFile, ImageFileDto, Guid, ImageFileGetListInput, ImageFileCreateUpdateDto, ImageFileCreateUpdateDto>,
    IImageFileAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.ImageFile.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.ImageFile.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.ImageFile.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.ImageFile.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.ImageFile.Delete;

    private readonly IImageFileRepository _repository;

    public ImageFileAppService(IImageFileRepository repository) : base(repository)
    {
        _repository = repository;
    }

    protected override async Task<IQueryable<ImageFile>> CreateFilteredQueryAsync(ImageFileGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            .WhereIf(!input.Path.IsNullOrWhiteSpace(), x => x.Path.Contains(input.Path))
            .WhereIf(!input.Extension.IsNullOrWhiteSpace(), x => x.Extension.Contains(input.Extension))
            .WhereIf(input.Size != null, x => x.Size == input.Size)
            .WhereIf(input.Width != null, x => x.Width == input.Width)
            .WhereIf(input.Height != null, x => x.Height == input.Height)
            .WhereIf(input.LibraryPathId != null, x => x.LibraryPathId == input.LibraryPathId)
            .WhereIf(input.ArchiveInfoPathId != null, x => x.ArchiveInfoPathId == input.ArchiveInfoPathId)
            .WhereIf(input.ComicChapterId != null, x => x.ComicChapterId == input.ComicChapterId)
            ;
    }
}
