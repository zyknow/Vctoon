﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using Vctoon.Comics;
using Vctoon.Hubs;
using Vctoon.Libraries;
using Vctoon.Services.Base;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace Vctoon.Services;

public class LibraryScanner(
    ILibraryRepository libraryRepository,
    IArchiveInfoRepository archiveInfoRepository,
    IImageFileRepository imageFileRepository,
    IHubContext<LibraryScanHub> libraryScanHub,
    ILogger<LibraryScanner> logger,
    IUnitOfWorkManager unitOfWorkManager,
    ImageFileScanner imageFileScanner,
    IServiceScopeFactory serviceScopeFactory,
    ComicManager comicManager,
    LibraryManager libraryManager
)
    : VctoonHubServiceBase(libraryScanHub), ITransientDependency

{
    private static object LockObject = new();
    
    private readonly int _maxDegreeOfParallelism = 1;
    public readonly List<string> ArchiveExtensions = [".zip", ".rar", ".cbz"];
    
    // TODO:  get extensions from settings
    public readonly List<string> ImageExtensions = [".jpg", ".png", ".jpeg", ".bmp", ".gif", ".webp"];
    
    public static List<Guid> ScanningLibraryId { get; set; } = [];
    
    public async Task ScannerAsync(Guid libraryId)
    {
        lock (LockObject)
        {
            if (ScanningLibraryId.Contains(libraryId))
            {
                logger.LogWarning($"Library {libraryId} is being scanned, please wait");
                return;
            }
            
            ScanningLibraryId.Add(libraryId);
        }
        
        try
        {
            IQueryable<Library>? query = (await libraryRepository.WithDetailsAsync(x => x.Paths)).Where(x => x.Id == libraryId);
            Library? library =
                await AsyncExecuter.FirstOrDefaultAsync(query);
            if (library is null)
            {
                throw new BusinessException("Library is not found");
            }
            
            logger.LogInformation($"Start scanning library: {library.Name}");
            
            await SendLibraryScanMessageAsync(libraryId, L["ScanningLibraryPathDirectory"]);
            await ScanningLibraryPathDirectoryStructureAsync(library);
            
            await libraryRepository.UpdateAsync(library, true);
            
            await SendLibraryScanMessageAsync(libraryId, L["ScanningLibraryPathFiles"]);
            
            await ScanningLibraryFileAsync(library);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Scanning library {libraryId} failed");
            throw;
        }
        finally
        {
            ScanningLibraryId.Remove(libraryId);
            await SendLibraryScannedAsync(libraryId);
            logger.LogInformation($"End scanning library: {libraryId}");
        }
    }
    
    private async Task ScanningLibraryFileAsync(Library library)
    {
        using SemaphoreSlim? semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);
        List<Task>? scanLibraryPathFileTasks = new List<Task>();
        
        foreach (var libraryPath in library.Paths)
        {
            await semaphore.WaitAsync();
            
            Task? task = Task.Run(async () =>
            {
                try
                {
                    using (IUnitOfWork? uow = unitOfWorkManager.Begin(true))
                    {
                        await using AsyncServiceScope scope = uow.ServiceProvider.CreateAsyncScope();
                        LibraryScanner? libraryScanner = scope.ServiceProvider.GetRequiredService<LibraryScanner>();
                        // do not auto save
                        await libraryScanner.ScanningLibraryPathFileAsync(libraryPath, true);
                    }
                }
                catch (Exception e)
                {
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            scanLibraryPathFileTasks.Add(task);
        }
        
        await Task.WhenAll(scanLibraryPathFileTasks);
        
        await libraryManager.DeleteLibraryEmptyComicAsync(library);
        
        await libraryRepository.UpdateAsync(library, true);
    }
    
    private async Task ScanningLibraryPathDirectoryStructureAsync(Library library)
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
    private bool NeedScanLibraryPathFiles(LibraryPath libraryPath)
    {
        return libraryPath.LastResolveTime is null ||
               Directory.GetLastWriteTimeUtc(libraryPath.Path) > libraryPath.LastResolveTime;
    }
    
    private async Task ScanningLibraryPathFileAsync(LibraryPath libraryPath, bool autoSave = false)
    {
        if (NeedScanLibraryPathFiles(libraryPath))
        {
            var filePaths = Directory.GetFiles(libraryPath.Path, "*.*", SearchOption.TopDirectoryOnly);
            
            await SendLibraryScanMessageAsync(libraryPath.LibraryId, L["ScanningLibraryPathDirectory"], libraryPath.Path);
            
            var imageFilePaths =
                filePaths.Where(x => ImageExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();
            
            await imageFileScanner.ScanByLibraryPathAsync(libraryPath, imageFilePaths, autoSave);
            
            var archiveFilePaths =
                filePaths.Where(x => ArchiveExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();
            
            await SendLibraryScanMessageAsync(libraryPath.LibraryId, L["ScanningArchiveInfo"], libraryPath.Path);
            
            
            List<Guid> deleteArchiveIds =
                await AsyncExecuter.ToListAsync(
                    (await archiveInfoRepository.GetQueryableAsync())
                    .Where(x => x.LibraryPathId == libraryPath.Id)
                    .Where(x => !archiveFilePaths.Contains(x.Path))
                    .Select(x => x.Id));
            
            if (!deleteArchiveIds.IsNullOrEmpty())
            {
                await archiveInfoRepository.DeleteManyAsync(deleteArchiveIds, autoSave);
            }
            
            foreach (var archiveFilePath in archiveFilePaths)
            {
                ArchiveInfo archiveInfo =
                    await ScanningArchiveInfoDirectoryStructureAsync(archiveFilePath, libraryPath.Id, autoSave);
                
                await imageFileScanner.ScanByArchiveInfoAsync(archiveInfo, libraryPath.LibraryId, autoSave);
            }
            
            libraryPath.LastModifyTime = Directory.GetLastWriteTimeUtc(libraryPath.Path);
            libraryPath.LastResolveTime = DateTime.UtcNow;
        }
    }
    
    private bool NeedScanArchiveInfo(ArchiveInfo archiveInfo)
    {
        return archiveInfo.LastResolveTime is null ||
               File.GetLastWriteTimeUtc(archiveInfo.Path) > archiveInfo.LastResolveTime;
    }
    
    
    private async Task<ArchiveInfo> ScanningArchiveInfoDirectoryStructureAsync(string archivePath, Guid libraryPathId,
        bool autoSave = false)
    {
        var query = await archiveInfoRepository.WithDetailsAsync(x => x.Paths);
        
        
        var archiveInfo =
            await AsyncExecuter.FirstOrDefaultAsync(query, x =>
                x.Path == archivePath && x.LibraryPathId == libraryPathId);
        
        if (archiveInfo is null)
        {
            archiveInfo = new ArchiveInfo(GuidGenerator.Create(), archivePath, Path.GetExtension(archivePath), libraryPathId);
            await archiveInfoRepository.InsertAsync(archiveInfo, autoSave);
        }
        
        if (!NeedScanArchiveInfo(archiveInfo))
        {
            return archiveInfo;
        }
        
        using var archive = ArchiveFactory.Open(archiveInfo.Path);
        var archiveDirs = archive.Entries.Where(x => x.IsDirectory).ToList();
        
        List<string> dirs = archiveDirs.Select(x => x.Key).ToList();
        dirs.Insert(0, null);
        
        // 查询出需要删除的目录
        var deleteArchivePaths = archiveInfo.Paths.Where(x => !dirs.Contains(x.Path)).ToList();
        archiveInfo.Paths.RemoveAll(x => deleteArchivePaths.Contains(x));
        
        // 查询出需要添加的目录
        var addArchivePaths = dirs.Where(x => !archiveInfo.Paths.Select(p => p.Path).Contains(x))
            .Select(dirPath =>
                new ArchiveInfoPath(GuidGenerator.Create(), dirPath,
                    archiveInfo.Id))
            .ToList();
        archiveInfo.Paths.AddRange(addArchivePaths);
        
        archiveInfoRepository.UpdateAsync(archiveInfo, autoSave);
        
        return archiveInfo;
    }
}