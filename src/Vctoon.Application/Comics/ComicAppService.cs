using Vctoon.Comics.Dtos;
using Vctoon.Permissions;
using Volo.Abp.Application.Services;

namespace Vctoon.Comics;

public class ComicAppService : CrudAppService<Comic, ComicDto, Guid, ComicGetListInput, ComicCreateUpdateDto,
        ComicCreateUpdateDto>,
    IComicAppService
{
    private readonly IComicRepository _repository;

    public ComicAppService(IComicRepository repository) : base(repository)
    {
        _repository = repository;
    }

    protected override string GetPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Comic.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Comic.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Comic.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Comic.Delete;

    protected override async Task<IQueryable<Comic>> CreateFilteredQueryAsync(ComicGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
            .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
            ;
    }
}