using Vctoon.Comics.Dtos;
using Vctoon.Permissions;
using Volo.Abp.Application.Services;

namespace Vctoon.Comics;

public class ComicChapterAppService : CrudAppService<ComicChapter, ComicChapterDto, Guid, ComicChapterGetListInput,
        ComicChapterCreateUpdateDto, ComicChapterCreateUpdateDto>,
    IComicChapterAppService
{
    private readonly IComicChapterRepository _repository;

    public ComicChapterAppService(IComicChapterRepository repository) : base(repository)
    {
        _repository = repository;
    }

    protected override string GetPolicyName { get; set; } = VctoonPermissions.ComicChapter.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.ComicChapter.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.ComicChapter.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.ComicChapter.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.ComicChapter.Delete;

    protected override async Task<IQueryable<ComicChapter>> CreateFilteredQueryAsync(ComicChapterGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
            .WhereIf(!input.CoverPath.IsNullOrWhiteSpace(), x => x.CoverPath.Contains(input.CoverPath))
            .WhereIf(input.PageCount != null, x => x.PageCount == input.PageCount)
            .WhereIf(input.Size != null, x => x.Size == input.Size)
            .WhereIf(input.ComicId != null, x => x.ComicId == input.ComicId)
            ;
    }
}