using Vctoon.Libraries.Dtos;
using Vctoon.Permissions;
using Vctoon.Services;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Libraries;

public class LibraryAppService : CrudAppService<Library, LibraryDto, Guid, LibraryGetListInput, LibraryCreateUpdateDto,
        LibraryCreateUpdateDto>,
    ILibraryAppService
{
    private readonly ILibraryPathRepository _libraryPathRepository;
    private readonly ILibraryRepository _repository;
    private readonly Scanner _scanner;

    public LibraryAppService(ILibraryRepository repository, Scanner scanner,
        ILibraryPathRepository libraryPathRepository) : base(repository)
    {
        _repository = repository;
        _scanner = scanner;
        _libraryPathRepository = libraryPathRepository;
    }

    protected override string GetPolicyName { get; set; } = VctoonPermissions.Library.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Library.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Library.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Library.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Library.Delete;
    protected string ScanPolicyName => CreatePolicyName;

    public override async Task<LibraryDto> CreateAsync(LibraryCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();

        // check has same name
        var library = await _repository.FirstOrDefaultAsync(x => x.Name == input.Name);
        if (library != null)
        {
            throw new UserFriendlyException("Library name already exists");
        }

        var entity = await MapToEntityAsync(input);

        TryToSetTenantId(entity);

        await Repository.InsertAsync(entity, true);

        var libraryDto = await MapToGetOutputDtoAsync(entity);

        var libraryPaths = input.Paths.Select(x => new LibraryPath(GuidGenerator.Create(), x, true, libraryDto.Id))
            .ToList();
        await _libraryPathRepository.InsertManyAsync(libraryPaths);

        libraryDto.Paths = input.Paths;

        return libraryDto;
    }

    public override async Task<LibraryDto> UpdateAsync(Guid id, LibraryCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();

        var library = await _repository.FirstOrDefaultAsync(x => x.Name == input.Name && x.Id != id);
        if (library != null)
        {
            throw new UserFriendlyException("Library name already exists");
        }

        var entity = await MapToEntityAsync(input);

        TryToSetTenantId(entity);

        await Repository.InsertAsync(entity, true);

        var libraryDto = await MapToGetOutputDtoAsync(entity);

        var libraryPaths = (await _libraryPathRepository.GetQueryableAsync())
            .Where(x => x.IsRoot).ToList();

        var removePaths = libraryPaths.Where(x => !input.Paths.Contains(x.Path)).ToList();

        await _libraryPathRepository.DeleteManyAsync(removePaths);

        var addPaths = input.Paths.Where(x => !libraryPaths.Select(x => x.Path).Contains(x))
            .Select(x => new LibraryPath(GuidGenerator.Create(), x, true, id)).ToList();

        await _libraryPathRepository.InsertManyAsync(addPaths);

        libraryDto.Paths = input.Paths;
        return libraryDto;
    }


    public async Task ScanAsync(Guid libraryId)
    {
        await CheckPolicyAsync(ScanPolicyName);
        await _scanner.ScannerAsync(libraryId);
    }

    protected override async Task<IQueryable<Library>> CreateFilteredQueryAsync(LibraryGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await Repository.WithDetailsAsync(x => x.Paths.Where(x => x.IsRoot)))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }
}