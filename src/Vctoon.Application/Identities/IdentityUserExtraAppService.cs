using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Vctoon.Identities.Dtos;
using Vctoon.Libraries;
using Vctoon.Libraries.Dtos;
using Vctoon.Services;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Vctoon.Identities;

[Authorize]
public class IdentityUserExtraAppService(
    IdentityUserManager userManager,
    IIdentityUserRepository userRepository,
    IIdentityRoleRepository roleRepository,
    IOptions<IdentityOptions> identityOptions,
    IPermissionChecker permissionChecker,
    IIdentityUserExtraRepository repository,
    ILibraryPermissionRepository libraryPermissionRepository,
    ILibraryPermissionChecker libraryPermissionChecker,
    ILibraryPermissionStore libraryPermissionStore,
    IIdentityUserRepository identityUserRepository,
    ILibraryRepository libraryRepository,
    IdentityUserStore identityUserStore) : VctoonAppService(libraryPermissionChecker), IIdentityUserExtraAppService
{
    private readonly ILibraryPermissionRepository _libraryPermissionRepository = libraryPermissionRepository;
    private readonly IIdentityUserExtraRepository _repository = repository;

    [Authorize(IdentityPermissions.Users.ManagePermissions)]
    public async Task<PagedResultDto<IdentityUserExtraDto>> GetUserExtraListAsync(GetIdentityUsersInput input)
    {
        long count = await userRepository.GetCountAsync(input.Filter);
        List<IdentityUser>? list =
            await userRepository.GetListAsync(input.Sorting, input.MaxResultCount, input.SkipCount, input.Filter);

        List<IdentityUserExtraDto> result = new List<IdentityUserExtraDto>();

        foreach (IdentityUser user in list)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);

            IdentityUserExtraDto userExtraP = ObjectMapper.Map<IdentityUser, IdentityUserExtraDto>(user);
            userExtraP.IsAdmin = roles.Contains(VctoonSharedConsts.AdminUserRoleName);
            result.Add(userExtraP);
        }

        return new PagedResultDto<IdentityUserExtraDto>(
            count,
            result
        );
    }

    [Authorize(IdentityPermissions.Users.Create)]
    public async Task<IdentityUserExtraDto> CreateUserExtraAsync(IdentityUserExtraCreateDto input)
    {
        IdentityUser user = ObjectMapper.Map<IdentityUserExtraCreateDto, IdentityUser>(input);

        await userManager.CreateAsync(user);

        if (input.IsAdmin)
        {
            await userManager.AddToRoleAsync(user, VctoonSharedConsts.AdminUserRoleName);
        }
        else if (!input.LibraryPermissions.IsNullOrEmpty())
        {
            foreach (LibraryPermissionDto permissionDto in input.LibraryPermissions)
            {
                LibraryPermission permission = new LibraryPermission(permissionDto.LibraryId, user.Id, permissionDto.CanDownload,
                    permissionDto.CanComment, permissionDto.CanStar, permissionDto.CanView, permissionDto.CanShare);
                await libraryPermissionStore.SetAsync(permission);
            }

            await userManager.AddToRoleAsync(user, VctoonSharedConsts.NormalUserRoleName);
        }


        IdentityUserExtraDto result = ObjectMapper.Map<IdentityUser, IdentityUserExtraDto>(user);
        result.IsAdmin = input.IsAdmin;
        result.LibraryPermissions = input.LibraryPermissions;

        return result;
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public async Task<IdentityUserExtraDto> UpdateUserExtraAsync(Guid userId,
        IdentityUserExtraUpdateDto input)
    {
        IdentityUser? user = await userManager.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserFriendlyException("User not found");
        }

        IList<string> userRoles = await userManager.GetRolesAsync(user);


        ObjectMapper.Map(input, user);

        await userManager.UpdateAsync(user);

        if (input.IsAdmin)
        {
            if (!userRoles.Contains(VctoonSharedConsts.AdminUserRoleName))
            {
                await userManager.AddToRoleAsync(user, VctoonSharedConsts.AdminUserRoleName);
            }
        }
        else
        {
            if (userRoles.Contains(VctoonSharedConsts.AdminUserRoleName))
            {
                await userManager.RemoveFromRoleAsync(user, VctoonSharedConsts.AdminUserRoleName);
            }

            if (!userRoles.Contains(VctoonSharedConsts.NormalUserRoleName))
            {
                await userManager.AddToRoleAsync(user, VctoonSharedConsts.NormalUserRoleName);
            }

            foreach (LibraryPermissionDto permissionDto in input.LibraryPermissions)
            {
                LibraryPermission permission = new LibraryPermission(permissionDto.LibraryId, user.Id, permissionDto.CanDownload,
                    permissionDto.CanComment, permissionDto.CanStar, permissionDto.CanView, permissionDto.CanShare);
                await libraryPermissionStore.SetAsync(permission);
            }
        }

        IdentityUserExtraDto result = ObjectMapper.Map<IdentityUser, IdentityUserExtraDto>(user);
        result.IsAdmin = input.IsAdmin;
        result.LibraryPermissions = input.LibraryPermissions;

        return result;
    }

    [Authorize(IdentityPermissions.Users.Create)]
    [Authorize(IdentityPermissions.Users.Update)]
    public async Task<List<LibraryPermissionDto>> GetPermissionsAsync(Guid userId)
    {
        IdentityUser? user = await identityUserRepository.GetAsync(userId);

        if (user == null)
        {
            throw new UserFriendlyException("User not found");
        }

        List<Guid> libraryIds = await AsyncExecuter.ToListAsync((await libraryRepository.GetQueryableAsync()).Select(x => x.Id));

        List<LibraryPermissionCacheItem> cacheItems = new List<LibraryPermissionCacheItem>();

        foreach (Guid libraryId in libraryIds)
        {
            LibraryPermissionCacheItem? permissionCache = await libraryPermissionStore.GetOrNullAsync(userId, libraryId);
            if (permissionCache != null)
            {
                cacheItems.Add(permissionCache);
            }
        }

        return ObjectMapper.Map<List<LibraryPermissionCacheItem>, List<LibraryPermissionDto>>(cacheItems);
    }

    [Authorize(IdentityPermissions.Users.ManagePermissions)]
    public async Task<IdentityUserExtraDto> GetUserExtraAsync(Guid userId)
    {
        IdentityUser? user = await identityUserRepository.GetAsync(userId);
        if (user == null)
        {
            throw new UserFriendlyException("User not found");
        }

        IList<string> roles = await userManager.GetRolesAsync(user);


        List<LibraryPermissionDto> permissions = await GetPermissionsAsync(userId);
        IdentityUserExtraDto result = ObjectMapper.Map<IdentityUser, IdentityUserExtraDto>(user);
        result.LibraryPermissions = permissions;
        result.IsAdmin = roles.Contains(VctoonSharedConsts.AdminUserRoleName);
        return result;
    }

    public async Task<List<LibraryPermissionDto>> GetCurrentUserLibraryPermissionsAsync()
    {
        Guid? userId = CurrentUser.Id;

        if (!userId.HasValue)
        {
            throw new UserFriendlyException("User not found");
        }

        return await GetPermissionsAsync(userId.Value);
    }

    [Authorize(IdentityPermissions.Users.Create)]
    public async Task UpdatePermissionsAsync(Guid userId, List<LibraryPermissionCreateUpdateDto> input)
    {
        IdentityUser? user = await identityUserRepository.GetAsync(userId);

        if (user == null)
        {
            throw new UserFriendlyException("User not found");
        }

        bool isAdmin = (await identityUserStore.GetRolesAsync(user)).Contains(VctoonSharedConsts.AdminUserRoleName);

        if (isAdmin)
        {
            throw new UserFriendlyException("Admin user can't set library permissions");
        }

        foreach (LibraryPermissionCreateUpdateDto permissionDto in input)
        {
            LibraryPermission permission = await libraryPermissionRepository.FirstOrDefaultAsync(x =>
                x.UserId == userId && x.LibraryId == permissionDto.LibraryId) ?? new LibraryPermission(permissionDto.LibraryId,
                userId, permissionDto.CanDownload,
                permissionDto.CanComment, permissionDto.CanStar, permissionDto.CanView, permissionDto.CanShare);

            await libraryPermissionStore.SetAsync(permission);
        }
    }


    [Authorize(IdentityPermissions.Users.Delete)]
    public async Task DeleteUserExtraAsync(Guid userId)
    {
        IdentityUser? user = await identityUserRepository.GetAsync(userId);
        if (user == null)
        {
            throw new UserFriendlyException("User not found");
        }

        await libraryPermissionStore.RemoveUserPermissionAsync(userId);

        await userManager.DeleteAsync(user);
    }
}