using Vctoon.Identities.Dtos;

namespace Vctoon.Identities;

public class IdentityUserLibraryPermissionGrantAppService : CrudAppService<IdentityUserLibraryPermissionGrant,
        IdentityUserLibraryPermissionGrantDto, Guid, IdentityUserLibraryPermissionGrantGetListInput,
        IdentityUserLibraryPermissionGrantCreateUpdateDto, IdentityUserLibraryPermissionGrantCreateUpdateDto>,
    IIdentityUserLibraryPermissionGrantAppService
{
    private readonly IIdentityUserLibraryPermissionGrantRepository _repository;
    
    public IdentityUserLibraryPermissionGrantAppService(IIdentityUserLibraryPermissionGrantRepository repository) :
        base(repository)
    {
        _repository = repository;
    }
    
    protected override string GetPolicyName { get; set; } = VctoonPermissions.IdentityUserLibraryPermissionGrant.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.IdentityUserLibraryPermissionGrant.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.IdentityUserLibraryPermissionGrant.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.IdentityUserLibraryPermissionGrant.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.IdentityUserLibraryPermissionGrant.Delete;
    
    protected override async Task<IQueryable<IdentityUserLibraryPermissionGrant>> CreateFilteredQueryAsync(
        IdentityUserLibraryPermissionGrantGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.UserId != null, x => x.UserId == input.UserId)
            .WhereIf(input.IsAdmin != null, x => x.IsAdmin == input.IsAdmin)
            ;
    }
}