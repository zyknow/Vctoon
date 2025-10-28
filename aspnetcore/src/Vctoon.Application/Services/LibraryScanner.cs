using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vctoon.Handlers;
using Vctoon.Mediums;
using Vctoon.Mediums.Base;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Vctoon.Services;

public class LibraryScanner(
    ILibraryRepository libraryRepository,
    ILogger<LibraryScanner> logger,
    IUnitOfWorkManager unitOfWorkManager,
    LibraryStore libraryStore,
    IEnumerable<IMediumScanHandler> mediumScanHandlers,
    MediumManager mediumManager)
    : VctoonService, IScopedDependency
{
    private static readonly object LockObject = new();

    private readonly int _maxDegreeOfParallelism = 8;

    public static List<Guid> ScanningLibraryIds { get; set; } = [];

    public async Task ScanAsync(Guid libraryId)
    {
        var stopwatch = Stopwatch.StartNew();

        var skipScan = false;
        lock (LockObject)
        {
            if (ScanningLibraryIds.Contains(libraryId))
            {
                logger.LogWarning("Library {LibraryId} is being scanned, please wait", libraryId);
                skipScan = true;
            }
            else
            {
                ScanningLibraryIds.Add(libraryId);
            }
        }

        if (skipScan)
        {
            return;
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

            logger.LogInformation("Start scanning library: {LibraryName}", library.Name);

            await SendLibraryScanMessageAsync(libraryId, L["ScanningLibraryPathDirectory"]);
            await ScanLibraryPathDirectoryStructureAsync(library);

            await libraryRepository.UpdateAsync(library, true);

            await SendLibraryScanMessageAsync(libraryId, L["ScanningLibraryPathFiles"]);

            await ScanLibraryFileAsync(library);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Scanning library {LibraryId} failed", libraryId);
            throw;
        }
        finally
        {
            lock (LockObject)
            {
                ScanningLibraryIds.Remove(libraryId);
            }

            await SendLibraryScannedAsync(libraryId);
            stopwatch.Stop();
            logger.LogInformation("End scanning library: {LibraryId}. Elapsed: {ElapsedMs} ms", libraryId,
                stopwatch.ElapsedMilliseconds);
        }
    }

    protected virtual async Task ScanLibraryFileAsync(Library library)
    {
        // 并行扫描 LibraryPath 文件，限制最大并行度
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxDegreeOfParallelism
        };

        await Parallel.ForEachAsync(library.Paths, parallelOptions, async (libraryPath, _) =>
        {
            try
            {
                using (var uow = unitOfWorkManager.Begin(true))
                {
                    await using var scope = uow.ServiceProvider.CreateAsyncScope();
                    var libraryScanner = scope.ServiceProvider.GetRequiredService<LibraryScanner>();
                    await libraryScanner.ScanningLibraryPathFileAsync(library, libraryPath);
                    await uow.CompleteAsync(_);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Scanning library path {Path} failed", libraryPath.Path);
            }
        });

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

        List<LibraryPath> deleteLibraryPaths = [];

        foreach (var libraryPath in rootPaths)
        {
            if (!Directory.Exists(libraryPath.Path))
            {
                deleteLibraryPaths.AddRange(library.Paths.Where(x => x.Path.StartsWith(libraryPath.Path)));
                // 不存在则跳过该根路径，继续处理其它根路径
                continue;
            }

            // 使用枚举器以降低内存峰值
            var dirsEnum = Directory.EnumerateDirectories(libraryPath.Path, "*", SearchOption.AllDirectories);
            var dirsSet = new HashSet<string>(dirsEnum, StringComparer.OrdinalIgnoreCase);

            var allPathsSet = new HashSet<string>(library.Paths.Select(p => p.Path), StringComparer.OrdinalIgnoreCase);

            deleteLibraryPaths.AddRange(library.Paths
                .Where(x => !x.IsRoot)
                .Where(x => !dirsSet.Contains(x.Path))
                .ToList());

            var addLibraryPaths = dirsSet
                .Where(x => !allPathsSet.Contains(x))
                .Select(dirPath =>
                    new LibraryPath(GuidGenerator.Create(), dirPath, false, library.Id))
                .ToList();

            library.Paths.AddRange(addLibraryPaths);
        }

        if (!deleteLibraryPaths.IsNullOrEmpty())
        {
            library.Paths.RemoveAll(x => deleteLibraryPaths.Contains(x));
            await mediumManager.DeleteMediumByLibraryPathIdsAsync(deleteLibraryPaths.Select(x => x.Id));


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