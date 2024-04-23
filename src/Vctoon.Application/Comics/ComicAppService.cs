using Vctoon.Comics.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Comics;

public class ComicAppService(IComicRepository repository)
    : CrudAppService<Comic, ComicDto, Guid, ComicGetListInput, ComicCreateUpdateDto,
            ComicCreateUpdateDto>(repository),
        IComicAppService
{
    private readonly IComicRepository _repository = repository;

    protected override string GetPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Comic.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Comic.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Comic.Delete;

    public override async Task<PagedResultDto<ComicDto>> GetListAsync(ComicGetListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = new List<Comic>();
        var entityDtos = new List<ComicDto>();

        if (totalCount > 0)
        {
            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            entities = await AsyncExecuter.ToListAsync(query);
            entityDtos = await MapToGetListOutputDtosAsync(entities);
        }


        return new PagedResultDto<ComicDto>(
            totalCount,
            entityDtos
        );
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