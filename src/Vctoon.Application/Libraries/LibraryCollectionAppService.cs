using System;
using System.Linq;
using System.Threading.Tasks;
using Vctoon.Permissions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Libraries;

public class LibraryCollectionAppService : CrudAppService<Favorite, LibraryCollectionDto, Guid,
        LibraryCollectionGetListInput, CreateUpdateLibraryCollectionDto, CreateUpdateLibraryCollectionDto>,
    ILibraryCollectionAppService
{
    private readonly IFavoriteRepository _repository;

    public LibraryCollectionAppService(IFavoriteRepository repository) : base(repository)
    {
        _repository = repository;
    }

    protected override string GetPolicyName { get; set; } = VctoonPermissions.Favorite.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Favorite.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Favorite.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Favorite.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Favorite.Delete;

    protected override async Task<IQueryable<Favorite>> CreateFilteredQueryAsync(LibraryCollectionGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            .WhereIf(!input.CoverPath.IsNullOrWhiteSpace(), x => x.CoverPath.Contains(input.CoverPath))
            ;
    }
}