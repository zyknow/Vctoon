using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vctoon.Handlers;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Vctoon.Services;

public class LibraryScanner(
    ILibraryRepository libraryRepository,
    ILogger<LibraryScanner> logger,
    IUnitOfWorkManager unitOfWorkManager,
    LibraryStore libraryStore,
    IEnumerable<IMediumScanHandler> mediumScanHandlers)
    : VctoonService, ITransientDependency

{
    private static readonly Lock LockObject = new();

    private readonly int _maxDegreeOfParallelism = 1;

    public static List<Guid> ScanningLibraryIds { get; set; } = [];

    public async Task ScanAsync(Guid libraryId)
    {
        lock (LockObject)
        {
            if (ScanningLibraryIds.Contains(libraryId))
            {
                logger.LogWarning($"Library {libraryId} is being scanned, please wait");
                return;
            }

            ScanningLibraryIds.Add(libraryId);
        }

        try
        {
            var query =
                (await libraryRepository.WithDetailsAsync(x => x.Paths)).Where(x => x.Id == libraryId);
            var library =
                await AsyncExecuter.FirstOrDefaultAsync(query);
            if (library is null)
            {
                throw new BusinessException("Library is not found");
            }

            logger.LogInformation($"Start scanning library: {library.Name}");

            await SendLibraryScanMessageAsync(libraryId, L["ScanningLibraryPathDirectory"]);
            await ScanLibraryPathDirectoryStructureAsync(library);

            await libraryRepository.UpdateAsync(library, true);

            await SendLibraryScanMessageAsync(libraryId, L["ScanningLibraryPathFiles"]);

            await ScanLibraryFileAsync(library);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Scanning library {libraryId} failed");
            throw;
        }
        finally
        {
            ScanningLibraryIds.Remove(libraryId);
            await SendLibraryScannedAsync(libraryId);
            logger.LogInformation($"End scanning library: {libraryId}");
        }
    }

    protected virtual async Task ScanLibraryFileAsync(Library library)
    {
        using var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);
        var scanLibraryPathFileTasks = new List<Task>();

        foreach (var libraryPath in library.Paths)
        {
            await semaphore.WaitAsync();

            var task = Task.Run(async () =>
            {
                try
                {
                    using (var uow = unitOfWorkManager.Begin(true))
                    {
                        await using var scope = uow.ServiceProvider.CreateAsyncScope();
                        var libraryScanner = scope.ServiceProvider.GetRequiredService<LibraryScanner>();
                        // do not auto save
                        await libraryScanner.ScanningLibraryPathFileAsync(library, libraryPath, true);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Scanning library path {libraryPath.Path} failed");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            scanLibraryPathFileTasks.Add(task);
        }

        await Task.WhenAll(scanLibraryPathFileTasks);

        await libraryStore.DeleteLibraryEmptyComicAsync(library);

        await libraryRepository.UpdateAsync(library, true);
    }

    protected virtual async Task ScanLibraryPathDirectoryStructureAsync(Library library)
    {
        // Scan only the directory structure, if it does not exist, create and add it to the Children of libraryPath, and you need to delete the non-existent Children

        var rootPaths = library.Paths.Where(x => x.IsRoot).ToList();

        if (rootPaths.IsNullOrEmpty())
        {
            return;
        }

        foreach (var libraryPath in rootPaths)
        {
            if (!Directory.Exists(libraryPath.Path))
            {
                return;
            }

            var dirs = Directory.GetDirectories(libraryPath.Path, "*", SearchOption.AllDirectories);

            var deleteLibraryPaths = library.Paths.Where(x => !x.IsRoot).Where(x => !dirs.Contains(x.Path)).ToList();
            library.Paths.RemoveAll(x => deleteLibraryPaths.Contains(x));

            var addLibraryPaths = dirs.Where(x => !library.Paths.Select(p => p.Path).Contains(x))
                .Select(dirPath =>
                    new LibraryPath(GuidGenerator.Create(), dirPath, false, library.Id))
                .ToList();

            library.Paths.AddRange(addLibraryPaths);
        }
    }

    /// <summary>
    /// Checks whether it is necessary to scan the files under LibraryPath.
    /// </summary>
    /// <param name="libraryPath"></param>
    /// <returns></returns>
    protected virtual bool NeedScanLibraryPathFiles(LibraryPath libraryPath)
    {
        return libraryPath.LastResolveTime is null ||
               Directory.GetLastWriteTimeUtc(libraryPath.Path) > libraryPath.LastResolveTime;
    }

    protected virtual async Task ScanningLibraryPathFileAsync(Library library, LibraryPath libraryPath,
        bool autoSave = false)
    {
        if (NeedScanLibraryPathFiles(libraryPath))
        {
            foreach (var mediumScanHandler in mediumScanHandlers)
            {
                await mediumScanHandler.ScanAsync(libraryPath, library.MediumType);
            }

            libraryPath.LastModifyTime = Directory.GetLastWriteTimeUtc(libraryPath.Path);
            libraryPath.LastResolveTime = DateTime.UtcNow;
        }
    }
}