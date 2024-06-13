using Vctoon.Libraries.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace Vctoon.Blazor.Client.Pages.Settings.UserManager;

public class IdentityUserExtraCreateOrUpdateModel : IdentityUserCreateOrUpdateDtoBase, IHasConcurrencyStamp
{
    public bool IsAdmin { get; set; }

    public List<LibraryPermissionDto> LibraryPermissions { get; set; }

    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
    public string Password { get; set; }

    public string ConcurrencyStamp { get; set; }
}