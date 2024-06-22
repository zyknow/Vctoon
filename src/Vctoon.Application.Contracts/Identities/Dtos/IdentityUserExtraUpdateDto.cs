﻿using Vctoon.Libraries.Dtos;
using Volo.Abp.Identity;

namespace Vctoon.Identities.Dtos;

public class IdentityUserExtraUpdateDto : IdentityUserUpdateDto
{
    public List<LibraryPermissionDto> LibraryPermissions { get; set; }
}