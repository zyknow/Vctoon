using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using Vctoon.Libraries;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Services;

public class Scanner(
    ILibraryRepository libraryRepository,
    IArchiveInfoRepository archiveInfoRepository,
    IImageFileRepository imageFileRepository,
    ILogger<Scanner> logger,
    ImageFileHandler imageFileHandler)
    : VctoonService, ITransientDependency

{
    public readonly List<string> ArchiveExtensions = [".zip", ".rar", ".cbz"];

    // TODO:  get extensions from settings
    public readonly List<string> ImageExtensions = [".jpg", ".png", ".jpeg", ".bmp", ".gif", ".webp"];

    public async Task ScannerAsync(Guid libraryId)
    {
        var query = (await libraryRepository.WithDetailsAsync(x => x.Paths)).Where(x => x.Id == libraryId);
        var library =
            await AsyncExecuter.FirstOrDefaultAsync(query);
        if (library is null)
        {
            throw new BusinessException("Library is not found");
        }

        await ScanningLibraryPathDirectoryStructureAsync(library);

        foreach (var libraryPath in library.Paths)
        {
            await ScanningLibraryPathFilesAsync(libraryPath);
        }

        await libraryRepository.UpdateAsync(library);
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

    private async Task ScanningLibraryPathFilesAsync(LibraryPath libraryPath)
    {
        if (NeedScanLibraryPathFiles(libraryPath))
        {
            var filePaths = Directory.GetFiles(libraryPath.Path, "*.*", SearchOption.TopDirectoryOnly);

            var imageFilePaths =
                filePaths.Where(x => ImageExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();

            await imageFileHandler.HandlerByLibraryPathAsync(libraryPath, imageFilePaths);


            var archiveFilePaths =
                filePaths.Where(x => ArchiveExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();

            foreach (var archiveFilePath in archiveFilePaths)
            {
                var archiveInfo = await ScanningArchiveInfoDirectoryStructureAsync(archiveFilePath);

                await imageFileHandler.HandlerByArchiveInfoAsync(archiveInfo, libraryPath.LibraryId);
            }

            // libraryPath.LastModifyTime = Directory.GetLastWriteTimeUtc(libraryPath.Path);
            // libraryPath.LastResolveTime = DateTime.UtcNow;
        }
    }

    private bool NeedScanArchiveInfo(ArchiveInfo archiveInfo)
    {
        return archiveInfo.LastResolveTime is null ||
               File.GetLastWriteTimeUtc(archiveInfo.Path) > archiveInfo.LastResolveTime;
    }


    private async Task<ArchiveInfo> ScanningArchiveInfoDirectoryStructureAsync(string archivePath)
    {
        var query = await archiveInfoRepository.WithDetailsAsync(x => x.Paths);


        var archiveInfo =
            await AsyncExecuter.FirstOrDefaultAsync(query, x =>
                x.Path == archivePath);

        if (archiveInfo is null)
        {
            archiveInfo = new ArchiveInfo(GuidGenerator.Create(), archivePath, Path.GetExtension(archivePath));
            await archiveInfoRepository.InsertAsync(archiveInfo);
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

        archiveInfoRepository.UpdateAsync(archiveInfo);

        return archiveInfo;
    }
}