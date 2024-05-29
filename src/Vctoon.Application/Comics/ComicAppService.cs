using Microsoft.AspNetCore.Authorization;
using Vctoon.Comics.Dtos;
using Vctoon.Libraries;
using Volo.Abp;

namespace Vctoon.Comics;

[Authorize]
public class ComicAppService(
    IComicRepository repository,
    IImageFileRepository imageFileRepository,
    IContentProgressRepository contentProgressRepository)
    : CrudAppService<Comic, ComicDto, Guid, ComicGetListInput, ComicCreateUpdateDto,
            ComicCreateUpdateDto>(repository),
        IComicAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Comic.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Comic.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Comic.Delete;
    
    
    protected override async Task<Comic> GetEntityByIdAsync(Guid id)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return await base.GetEntityByIdAsync(id);
        }
        
        Comic? comic = await AsyncExecuter.FirstOrDefaultAsync((await Repository.WithDetailsAsync(x => x.Progresses))
            .Where(x => x.Id == id));
        
        return comic;
    }
    
    public async Task UpdateProgressAsync(Guid id, double progress)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new UserFriendlyException("User is not authenticated");
        }
        
        ContentProgress? progressEntity =
            await contentProgressRepository.FindAsync(x => x.ComicId == id && x.UserId == CurrentUser.Id);
        progressEntity.CompletionRate = progress;
        await contentProgressRepository.UpdateAsync(progressEntity);
    }
    
    protected override async Task<IQueryable<Comic>> CreateFilteredQueryAsync(ComicGetListInput input)
    {
        // TODO: AbpHelper generated
        IQueryable<Comic> query = (await Repository.WithDetailsAsync(x => x.Progresses.Where(p => p.UserId == CurrentUser.Id)))
            .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
            .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId);
        return query;
    }
    
    protected override IQueryable<Comic> ApplySorting(IQueryable<Comic> query, ComicGetListInput input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            return base.ApplySorting(query, input);
        }
        
        if (input.Sorting.Contains(nameof(ComicDto.Progress), StringComparison.OrdinalIgnoreCase))
        {
            if (!CurrentUser.Id.HasValue)
            {
                return base.ApplySorting(query, input);
            }
            
            bool isDesc = input.Sorting.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
            query = isDesc
                ? query.OrderByDescending(x => x.Progresses.First(p => p.UserId == CurrentUser.Id).CompletionRate)
                : query.OrderBy(x => x.Progresses.First(p => p.UserId == CurrentUser.Id).CompletionRate);
            
            return query;
        }
        
        return base.ApplySorting(query, input);
    }
}