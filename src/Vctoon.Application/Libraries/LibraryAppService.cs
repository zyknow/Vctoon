using Vctoon.Libraries.Dtos;
using Vctoon.Localization.Libraries;
using Vctoon.Services;

namespace Vctoon.Libraries;

public class LibraryAppService : CrudAppService<Library, LibraryDto, Guid, LibraryGetListInput, LibraryCreateUpdateDto,
        LibraryCreateUpdateDto>,
    ILibraryAppService
{
    private readonly LibraryManager _libraryManager;
    private readonly LibraryScanner _libraryScanner;
    
    public LibraryAppService(
        ILibraryRepository repository,
        LibraryScanner libraryScanner,
        ILibraryPathRepository libraryPathRepository,
        LibraryManager libraryManager) : base(repository)
    {
        _libraryScanner = libraryScanner;
        _libraryManager = libraryManager;
        LocalizationResource = typeof(LibraryResource);
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
        
        Library? entity = await _libraryManager.CreateLibraryAsync(input.Name, input.Paths);
        return await MapToGetOutputDtoAsync(entity);
    }
    
    public override async Task<LibraryDto> UpdateAsync(Guid id, LibraryCreateUpdateDto input)
    {
        await CheckCreatePolicyAsync();
        
        var library =
            await AsyncExecuter.FirstOrDefaultAsync((await Repository.WithDetailsAsync()).Where(x => x.Id == id));
        
        library.SetName(input.Name);
        
        await _libraryManager.UpdateLibraryAsync(library);
        await _libraryManager.UpdateLibraryPathsAsync(library, input.Paths);
        
        return await MapToGetOutputDtoAsync(library);
    }
    
    
    public async Task ScanAsync(Guid libraryId)
    {
        await CheckPolicyAsync(ScanPolicyName);
        await _libraryScanner.ScannerAsync(libraryId);
    }
    
    protected override async Task<IQueryable<Library>> CreateFilteredQueryAsync(LibraryGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await Repository.WithDetailsAsync(x => x.Paths.Where(x => x.IsRoot)))
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            ;
    }
}