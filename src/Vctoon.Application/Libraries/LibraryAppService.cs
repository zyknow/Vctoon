using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;
using Vctoon.Services;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public class LibraryAppService(
    ILibraryRepository repository,
    Scanner scanner,
    ILibraryPathRepository libraryPathRepository,
    LibraryManager libraryManager)
    : CrudAppService<Library, LibraryDto, Guid, LibraryGetListInput, LibraryCreateUpdateDto,
            LibraryCreateUpdateDto>(repository),
        ILibraryAppService
{
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Library.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Library.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Library.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Library.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Library.Delete;
    protected string ScanPolicyName => CreatePolicyName;

    public override async Task<LibraryDto> CreateAsync(LibraryCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();

        var entity = await libraryManager.CreateLibraryAsync(input.Name, input.Paths);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<LibraryDto> UpdateAsync(Guid id, LibraryCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();

        var library =
            await AsyncExecuter.FirstOrDefaultAsync((await Repository.WithDetailsAsync()).Where(x => x.Id == id));
        if (library != null)
        {
            throw new UserFriendlyException("Library name already exists");
        }

        library.SetName(input.Name);

        await libraryManager.UpdateLibraryAsync(library);
        await libraryManager.UpdateLibraryPathsAsync(library, input.Paths);

        return await MapToGetOutputDtoAsync(library);
    }


    public async Task ScanAsync(Guid libraryId)
    {
        await CheckPolicyAsync(ScanPolicyName);
        await scanner.ScannerAsync(libraryId);
    }

    protected override async Task<IQueryable<Library>> CreateFilteredQueryAsync(LibraryGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await Repository.WithDetailsAsync(x => x.Paths.Where(x => x.IsRoot)))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }
}