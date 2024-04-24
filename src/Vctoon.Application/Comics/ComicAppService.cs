using Microsoft.AspNetCore.Authorization;
using Vctoon.Comics.Dtos;
using Vctoon.Libraries;
using Vctoon.Services;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics;

[Authorize]
public class ComicAppService(
    IComicRepository repository,
    ContentProgressQueryService contentProgressQueryService,
    IContentProgressRepository contentProgressRepository,
    ComicChapterManager comicChapterManager)
    : CrudAppService<Comic, ComicDto, Guid, ComicGetListInput, ComicCreateUpdateDto,
            ComicCreateUpdateDto>(repository),
        IComicAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Comic.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Comic.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Comic.Delete;

    public override async Task<PagedResultDto<ComicDto>> GetListAsync(ComicGetListInput input)
    {
        var res = await base.GetListAsync(input);

        if (CurrentUser.Id is null)
        {
            return res;
        }

        await contentProgressQueryService.AppendCompletionRateAsync(CurrentUser.Id.Value, res.Items.ToList());

        return res;
    }

    /// <summary>
    ///   Change process status of a comic
    /// </summary>
    /// <param name="id"></param>
    /// <param name="toCompleted">
    /// true: will set comic and chapter is Completed.
    /// false: clear comic and chapter process. </param>
    /// <exception cref="UserFriendlyException"></exception>
    public async Task ChangeProcessStatusAsync(Guid id, bool toCompleted)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new UserFriendlyException("User is not authenticated");
        }

        if (!toCompleted)
        {
            var chapterIds = await AsyncExecuter.ToListAsync(
                (await Repository.WithDetailsAsync(x => x.Chapters))
                .SelectMany(x => x.Chapters
                    .Select(x => x.Id)));

            if (chapterIds.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Comic has no chapters");
            }

            await contentProgressRepository.DeleteAsync(x => chapterIds.Contains(x.ComicChapterId.Value));
            await contentProgressRepository.DeleteAsync(x => x.ComicId == id);
        }
        else
        {
            var chapters = await AsyncExecuter.ToListAsync(
                (await Repository.WithDetailsAsync(x => x.Chapters))
                .Where(x => x.Id == id)
                .SelectMany(x => x.Chapters));

            if (chapters.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Comic has no chapters");
            }

            foreach (var comicChapter in chapters)
            {
                await comicChapterManager.UpdateOrAddProcessAsync(comicChapter, CurrentUser.Id.Value, 100);
            }
        }
    }

    public override async Task<ComicDto> GetAsync(Guid id)
    {
        var comic = await base.GetAsync(id);

        if (CurrentUser.Id is null)
        {
            return comic;
        }

        await contentProgressQueryService.AppendCompletionRateAsync(CurrentUser.Id.Value, comic);

        return comic;
    }


    protected override async Task<IQueryable<Comic>> CreateFilteredQueryAsync(ComicGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
            .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
            ;
    }
}