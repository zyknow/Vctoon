using Microsoft.AspNetCore.Authorization;
using Vctoon.Extensions;
using Vctoon.Libraries.Dtos;
using Vctoon.Localization.Libraries;
using Vctoon.Services;
using Volo.Abp.BackgroundJobs;

namespace Vctoon.Libraries;

[Authorize]
public class LibraryAppService : VctoonAppService,
    ILibraryAppService
{
    private readonly LibraryManager _libraryManager;
    private readonly ILibraryPermissionRepository _libraryPermissionRepository;
    private readonly LibraryScanner _libraryScanner;
    private readonly ILibraryRepository _repository;

    public LibraryAppService(
        ILibraryRepository repository,
        LibraryScanner libraryScanner,
        LibraryManager libraryManager, IBackgroundJobManager backgroundJobManager,
        ILibraryPermissionRepository libraryPermissionRepository,
        ILibraryPermissionChecker libraryPermissionChecker) : base(
        libraryPermissionChecker)
    {
        _repository = repository;
        _libraryScanner = libraryScanner;
        _libraryManager = libraryManager;
        _libraryPermissionRepository = libraryPermissionRepository;
        LocalizationResource = typeof(LibraryResource);
    }

    [Authorize(VctoonPermissions.Library.Default)]
    public async Task<List<LibraryDto>> GetCurrentUserLibraryListAsync()
    {
        bool isAdmin = CurrentUser.IsAdmin();

        IEnumerable<Guid> libraryIds = isAdmin
            ? []
            : (await _libraryPermissionRepository.GetListAsync(x => x.UserId == CurrentUser.Id && x.CanView))
            .Select(x => x.LibraryId);

        List<Library> libraries =
            isAdmin ? await _repository.GetListAsync() : await _repository.GetListAsync(x => libraryIds.Contains(x.Id));

        List<LibraryDto>? dtos = ObjectMapper.Map<List<Library>, List<LibraryDto>>(libraries);
        return dtos;
    }

    [Authorize(VctoonPermissions.Library.Create)]
    public async Task<LibraryDto> CreateAsync(LibraryCreateUpdateDto input)
    {
        Library? entity = await _libraryManager.CreateLibraryAsync(input.Name, input.Paths);
        return ObjectMapper.Map<Library, LibraryDto>(entity);
    }

    [Authorize(VctoonPermissions.Library.Update)]
    public async Task<LibraryDto> UpdateAsync(Guid id, LibraryCreateUpdateDto input)
    {
        var library =
            await AsyncExecuter.FirstOrDefaultAsync((await _repository.WithDetailsAsync()).Where(x => x.Id == id));

        library.SetName(input.Name);

        await _libraryManager.UpdateLibraryAsync(library);
        await _libraryManager.UpdateLibraryPathsAsync(library, input.Paths);

        return ObjectMapper.Map<Library, LibraryDto>(library);
    }

    [Authorize(VctoonPermissions.Library.Update)]
    public async Task ScanAsync(Guid libraryId)
    {
        await _libraryScanner.ScannerAsync(libraryId);
    }

    [Authorize(VctoonPermissions.Library.Default)]
    public async Task<LibraryDto> GetAsync(Guid id)
    {
        IQueryable<Library>? query = await _repository.WithDetailsAsync(x => x.Paths.Where(p => p.IsRoot));

        Library library = await AsyncExecuter.FirstAsync(query);

        return ObjectMapper.Map<Library, LibraryDto>(library);
    }

    [Authorize(VctoonPermissions.Library.Delete)]
    public Task DeleteAsync(Guid id)
    {
        return _repository.DeleteAsync(id);
    }
}