using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
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
    ComicChapterManager comicChapterManager,
    IComicChapterRepository comicChapterRepository)
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
        await CheckGetListPolicyAsync();
        
        IQueryable<Comic>? query = await CreateFilteredQueryAsync(input);
        int totalCount = await AsyncExecuter.CountAsync(query);
        
        List<Comic> entities = new();
        List<ComicDto> entityDtos = new();
        
        if (totalCount > 0)
        {
            if (!input.Sorting.IsNullOrWhiteSpace() &&
                input.Sorting.Contains(nameof(ComicDto.Progress), StringComparison.OrdinalIgnoreCase))
            {
                query = query.order(x => x.Progresses.Where(x => x.UserId == CurrentUser.Id).First().CompletionRate);
            }
            else
            {
                query = ApplySorting(query, input);
            }
            
            query = ApplyPaging(query, input);
            
            entities = await AsyncExecuter.ToListAsync(query);
            entityDtos = await MapToGetListOutputDtosAsync(entities);
        }
        
        PagedResultDto<ComicDto> res = new(
            totalCount,
            entityDtos
        );
        
        // var res = await base.GetListAsync(input);
        
        if (CurrentUser.Id is null)
        {
            return res;
        }
        
        if (!res.Items.IsNullOrEmpty())
        {
            //await contentProgressQueryService.AppendCompletionRateAsync(CurrentUser.Id.Value, res.Items.ToList());
        }
        
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
    
    public async Task<List<ComicChapterDto>> GetAllChaptersAsync(Guid comicId)
    {
        // TODO: should check permission by chapter?
        await CheckGetListPolicyAsync();
        
        List<ComicChapter> chapters = await AsyncExecuter.ToListAsync(
            (await comicChapterRepository.GetQueryableAsync())
            .Where(x => x.ComicId == comicId)
        );
        
        List<ComicChapterDto> chapterDtos = ObjectMapper.Map<List<ComicChapter>, List<ComicChapterDto>>(chapters);
        
        return chapterDtos;
    }
    
    protected override async Task<IQueryable<Comic>> CreateFilteredQueryAsync(ComicGetListInput input)
    {
        // TODO: AbpHelper generated
        IQueryable<Comic> query = (await base.CreateFilteredQueryAsync(input))
                .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
                .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
            ;
        
        //if (!input.Sorting.IsNullOrWhiteSpace() && input.Sorting.Contains(nameof(ComicDto.Progress), StringComparison.OrdinalIgnoreCase))
        //{
        //    query = query.OrderBy(x => x.Progresses.Where(x => x.UserId == CurrentUser.Id).First().CompletionRate);
        //}
        
        return query;
    }
}