using Microsoft.Extensions.DependencyInjection;
using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Vctoon.Libraries;

[Authorize]
public class LibraryAppService(
    ILibraryRepository repository,
    LibraryStore libraryStore,
    LibraryPermissionStore libraryPermissionStore,
    ILibraryPermissionRepository libraryPermissionRepository,
    IdentityUserStore identityUserStore,
    IRepository<IdentityUser> identityUserRepository,
    IIdentityRoleRepository identityRoleRepository,
    IUnitOfWorkManager unitOfWorkManager)
    : VctoonAppService,
        ILibraryAppService
{
    [Authorize(VctoonPermissions.Library.Default)]
    public async Task<List<LibraryDto>> GetCurrentUserLibraryListAsync()
    {
        var libraryIds = (await libraryPermissionRepository.GetListAsync(x => x.UserId == CurrentUser.Id && x.CanView))
            .Select(x => x.LibraryId);

        var libraries =
            await repository.GetListAsync(x => libraryIds.Contains(x.Id), includeDetails: true);

        var dtos = ObjectMapper.Map<List<Library>, List<LibraryDto>>(libraries);
        return dtos;
    }

    [Authorize(VctoonPermissions.Library.Create)]
    public async Task<LibraryDto> CreateAsync(LibraryCreateUpdateDto input)
    {
        var entity = await libraryStore.CreateLibraryAsync(input.Name, input.MediumType, input.Paths);

        var admins = await identityUserStore.GetUsersInRoleAsync(VctoonSharedConsts.AdminUserRoleName.ToUpper());
        foreach (var identityUser in admins)
        {
            var permission = new LibraryPermission(
                entity.Id,
                identityUser.Id,
                true,
                true,
                true,
                true,
                true
            );
            await libraryPermissionStore.SetAsync(permission);
        }

        return ObjectMapper.Map<Library, LibraryDto>(entity);
    }

    [Authorize(VctoonPermissions.Library.Update)]
    public async Task<LibraryDto> UpdateAsync(Guid id, LibraryCreateUpdateDto input)
    {
        var library =
            await AsyncExecuter.FirstOrDefaultAsync((await repository.WithDetailsAsync()).Where(x => x.Id == id));

        if (library is null)
        {
            throw new BusinessException("library no found")
                .WithData("libraryId", id);
        }

        library.SetName(input.Name);

        await libraryStore.UpdateLibraryAsync(library);
        await libraryStore.UpdateLibraryPathsAsync(library, input.Paths);

        return ObjectMapper.Map<Library, LibraryDto>(library);
    }


    [Authorize(VctoonPermissions.Library.Update)]
    public async Task ScanAsync(Guid libraryId)
    {
        _ = Task.Run(async () =>
        {
            using (var uow = unitOfWorkManager.Begin(
                       true, false
                   ))
            {
                var services = uow.ServiceProvider;
                var libraryScanner = services.GetRequiredService<LibraryScanner>();
                await libraryScanner.ScanAsync(libraryId);
                await uow.CompleteAsync();
            }
        });
    }

    [Authorize(VctoonPermissions.Library.Default)]
    public async Task<LibraryDto> GetAsync(Guid id)
    {
        var query = await repository.WithDetailsAsync(x => x.Paths.Where(p => p.IsRoot));

        var library = await AsyncExecuter.FirstAsync(query);

        return ObjectMapper.Map<Library, LibraryDto>(library);
    }

    [Authorize(VctoonPermissions.Library.Delete)]
    public Task DeleteAsync(Guid id)
    {
        return repository.DeleteAsync(id);
    }

    [Authorize(VctoonPermissions.Library.Update)]
    public async Task<LibraryPermissionDto> GetLibraryPermissionsAsync(Guid userId, Guid libraryId)
    {
        var permission = await libraryPermissionStore.GetOrNullAsync(userId, libraryId);
        if (permission == null)
        {
            return new LibraryPermissionDto
            {
                UserId = userId,
                LibraryId = libraryId,
            };
        }

        return ObjectMapper.Map<LibraryPermission, LibraryPermissionDto>(permission);
    }


    [Authorize(VctoonPermissions.Library.Update)]
    public async Task UpdateLibraryPermissionsAsync(Guid userId, Guid libraryId, LibraryPermissionCreateUpdateDto input)
    {
        var identityUser = await identityUserRepository.FirstOrDefaultAsync(x => x.Id == userId);

        if (identityUser == null)
        {
            throw new BusinessException("identityUser not found")
                .WithData("userId", userId);
        }

        var userRoles = await identityUserStore.GetRolesAsync(identityUser);

        if (userRoles.Contains(VctoonSharedConsts.AdminUserRoleName))
        {
            throw new BusinessException(VctoonDomainErrorCodes.AdminCanNotChangePermission);
        }

        var permission = await libraryPermissionStore.GetOrNullAsync(userId, libraryId) ?? new LibraryPermission();
        ObjectMapper.Map(input, permission);
        permission.UserId = userId;
        permission.LibraryId = libraryId;

        await libraryPermissionStore.SetAsync(permission);
    }

    // getAssignableUsersAsync
    [Authorize(VctoonPermissions.Library.Update)]
    public async Task<IPagedResult<IdentityUserDto>> GetAssignableUsersAsync(GetIdentityUsersInput input)
    {
        var adminRole =
            await identityRoleRepository.FindByNormalizedNameAsync(VctoonSharedConsts.AdminUserRoleName.ToUpper());

        var query = (await identityUserRepository.GetQueryableAsync()).Where(x =>
            x.Roles.Any(r => r.RoleId != adminRole.Id));

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(),
                x => x.UserName.Contains(input.Filter) ||
                     x.Name.Contains(input.Filter) ||
                     x.Email.Contains(input.Filter))
            .PageBy(input.SkipCount, input.MaxResultCount);

        var users = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<IdentityUserDto>(
            totalCount,
            ObjectMapper.Map<List<IdentityUser>, List<IdentityUserDto>>(users)
        );
    }
}