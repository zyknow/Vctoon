using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;

namespace Vctoon.Libraries;

public class ArtistAppService(IArtistRepository repository)
    : CrudAppService<Artist, ArtistDto, Guid, ArtistGetListInput, ArtistCreateUpdateDto,
            ArtistCreateUpdateDto>(repository),
        IArtistAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Artist.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Artist.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Artist.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Artist.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Artist.Delete;

    protected override async Task<IQueryable<Artist>> CreateFilteredQueryAsync(ArtistGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            .WhereIf(!input.Description.IsNullOrWhiteSpace(), x => x.Description.Contains(input.Description))
            ;
    }
}