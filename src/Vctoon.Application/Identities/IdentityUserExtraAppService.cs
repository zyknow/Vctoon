using Vctoon.Identities.Dtos;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Identities;

public class IdentityUserExtraAppService : CrudAppService<IdentityUserExtra, IdentityUserExtraDto, Guid,
        PagedAndSortedResultRequestDto, IdentityUserExtraCreateUpdateDto, IdentityUserExtraCreateUpdateDto>,
    IIdentityUserExtraAppService
{
    private readonly IIdentityUserExtraRepository _repository;
    
    public IdentityUserExtraAppService(IIdentityUserExtraRepository repository) : base(repository)
    {
        _repository = repository;
    }
    
    protected override string GetPolicyName { get; set; } = VctoonPermissions.IdentityUserExtra.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.IdentityUserExtra.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.IdentityUserExtra.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.IdentityUserExtra.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.IdentityUserExtra.Delete;
}