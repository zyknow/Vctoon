using Microsoft.AspNetCore.Authorization;
using Vctoon.Libraries.Dtos;
using Volo.Abp;

namespace Vctoon.Libraries;

[Authorize]
public class ImageFileAppService :
    CrudAppService<ImageFile, ImageFileDto, Guid, ImageFileGetListInput, ImageFileCreateUpdateDto, ImageFileCreateUpdateDto>,
    IImageFileAppService
{
    public ImageFileAppService(IImageFileRepository repository) : base(repository)
    {
    }
    
    [Obsolete]
    [RemoteService(false)]
    public override Task<ImageFileDto> CreateAsync(ImageFileCreateUpdateDto input)
    {
        return base.CreateAsync(input);
    }
    
    [Obsolete]
    [RemoteService(false)]
    public override Task<ImageFileDto> UpdateAsync(Guid id, ImageFileCreateUpdateDto input)
    {
        return base.UpdateAsync(id, input);
    }
    
    protected override async Task<IQueryable<ImageFile>> CreateFilteredQueryAsync(ImageFileGetListInput input)
    {
        IQueryable<ImageFile>? query = await base.CreateFilteredQueryAsync(input);
        
        query = query
                .WhereIf(input.ComicId != null, x => x.ComicId == input.ComicId)
                .WhereIf(input.Extension != null, x => x.Extension == input.Extension)
                .WhereIf(input.LibraryPathId != null, x => x.LibraryPathId == input.LibraryPathId)
                .WhereIf(input.Name != null, x => x.Name == input.Name)
                .WhereIf(input.Path != null, x => x.Path == input.Path)
                .WhereIf(input.Size != null, x => x.Size == input.Size)
            ;
        
        return query;
    }
}