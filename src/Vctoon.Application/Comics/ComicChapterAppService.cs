using Microsoft.AspNetCore.Authorization;
using Vctoon.Comics.Dtos;
using Vctoon.Services;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics;

[Authorize]
public class ComicChapterAppService(
    IComicChapterRepository repository,
    ContentProgressQueryService contentProgressQueryService,
    ComicChapterManager comicChapterManager)
    : CrudAppService<ComicChapter, ComicChapterDto, Guid, ComicChapterGetListInput,
            ComicChapterCreateUpdateDto, ComicChapterCreateUpdateDto>(repository),
        IComicChapterAppService
{
    private readonly IComicChapterRepository _repository = repository;

    protected override string GetPolicyName { get; set; } = VctoonPermissions.ComicChapter.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.ComicChapter.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.ComicChapter.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.ComicChapter.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.ComicChapter.Delete;


    public override async Task<PagedResultDto<ComicChapterDto>> GetListAsync(ComicChapterGetListInput input)
    {
        var res = await base.GetListAsync(input);

        if (CurrentUser.Id is null)
        {
            return res;
        }

        await contentProgressQueryService.AppendCompletionRateAsync(CurrentUser.Id.Value, res.Items.ToList());

        return res;
    }

    public async Task UpdateProgressAsync(Guid id, double progress)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new UserFriendlyException("User is not authenticated");
        }

        var comicChapter = await _repository.GetAsync(id);
        comicChapterManager.UpdateOrAddProcessAsync(comicChapter, CurrentUser.Id.Value, progress);
    }


    public override async Task<ComicChapterDto> GetAsync(Guid id)
    {
        var comicChapter = await base.GetAsync(id);

        if (CurrentUser.Id is null)
        {
            return comicChapter;
        }

        await contentProgressQueryService.AppendCompletionRateAsync(CurrentUser.Id.Value, comicChapter);

        return comicChapter;
    }

    protected override async Task<IQueryable<ComicChapter>> CreateFilteredQueryAsync(ComicChapterGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
            .WhereIf(input.PageCount != null, x => x.PageCount == input.PageCount)
            .WhereIf(input.Size != null, x => x.Size == input.Size)
            .WhereIf(input.ComicId != null, x => x.ComicId == input.ComicId)
            ;
    }
}