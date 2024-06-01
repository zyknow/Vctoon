using Vctoon.Identities.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Vctoon.Identities;

public interface IIdentityUserExtraAppService :
    ICrudAppService<
        IdentityUserExtraDto,
        Guid,
        PagedAndSortedResultRequestDto,
        IdentityUserExtraCreateUpdateDto,
        IdentityUserExtraCreateUpdateDto>
{
}