using SharpCompress.Archives;
using Vctoon.Mediums;

namespace Vctoon.Handlers;

public class ComicScanHandler(
    IComicImageRepository comicImageRepository,
    IArchiveInfoRepository archiveInfoRepository,
    CoverSaver coverSaver,
    IComicRepository comicRepository)
    : VctoonService, IMediumScanHandler
{
    public readonly HashSet<string> ArchiveExtensions =
        new([".zip", ".rar", ".cbr", ".cbz", ".7z", ".cb7"], StringComparer.OrdinalIgnoreCase);

    // TODO:  get extensions from settings
    public readonly List<string> ImageExtensions =
        [".jpg", ".png", ".jpeg", ".bmp", ".gif", ".webp"];


    public virtual async Task ScanAsync(LibraryPath libraryPath, MediumType mediaType)
    {
        if (mediaType != MediumType.Comic)
        {
            return;
        }

        await SendLibraryScanMessageAsync(libraryPath.LibraryId, L["ScanningLibraryPathDirectory"],
            libraryPath.Path);

        var filePaths = Directory.GetFiles(libraryPath.Path, "*.*", SearchOption.TopDirectoryOnly);

        var imageFilePaths =
            filePaths.Where(x => ImageExtensions.Contains(Path.GetExtension(x).ToLowerInvariant())).ToList();

        await ScanImageByLibraryPathAsync(libraryPath, imageFilePaths, true);

        var archiveFilePaths =
            filePaths.Where(x => ArchiveExtensions.Contains(Path.GetExtension(x))).ToList();

        var deleteArchiveIds =
            await AsyncExecuter.ToListAsync(
                (await archiveInfoRepository.GetQueryableAsync())
                .Where(x => x.LibraryPathId == libraryPath.Id)
                .Where(x => !archiveFilePaths.Contains(x.Path))
                .Select(x => x.Id));

        if (!deleteArchiveIds.IsNullOrEmpty())
        {
            await archiveInfoRepository.DeleteManyAsync(deleteArchiveIds, true);
        }

        foreach (var archiveFilePath in archiveFilePaths)
        {
            var archiveInfo =
                await ScanArchiveInfoDirectoryStructureAsync(archiveFilePath, libraryPath.Id);

            await ScanImageByArchiveInfoAsync(archiveInfo, libraryPath.LibraryId);
        }
    }

    protected virtual async Task<ArchiveInfo> ScanArchiveInfoDirectoryStructureAsync(string archivePath,
        Guid libraryPathId)
    {
        var query = await archiveInfoRepository.WithDetailsAsync(x => x.Paths);


        var archiveInfo =
            await AsyncExecuter.FirstOrDefaultAsync(query, x =>
                x.Path == archivePath && x.LibraryPathId == libraryPathId);

        if (archiveInfo is null)
        {
            var fileInfo = new FileInfo(archivePath);
            var sizeInBytes = fileInfo.Length;
            archiveInfo = new ArchiveInfo(GuidGenerator.Create(), archivePath,
                sizeInBytes,
                libraryPathId);
            await archiveInfoRepository.InsertAsync(archiveInfo, true);
        }

        if (!NeedScanArchiveInfo(archiveInfo))
        {
            return archiveInfo;
        }

        using var archive = ArchiveFactory.Open(archiveInfo.Path);
        var archiveDirs = archive.Entries.Where(x => x.IsDirectory).ToList();

        List<string?> dirs = archiveDirs.Select(x => x.Key).ToList();
        dirs.Insert(0, (string?)null);

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

        await archiveInfoRepository.UpdateAsync(archiveInfo, true);

        return archiveInfo;
    }

    protected virtual bool NeedScanArchiveInfo(ArchiveInfo archiveInfo)
    {
        return archiveInfo.LastResolveTime is null ||
               File.GetLastWriteTimeUtc(archiveInfo.Path) > archiveInfo.LastResolveTime;
    }


    public async Task ScanImageByLibraryPathAsync(LibraryPath libraryPath, List<string> imageFilePaths,
        bool autoSave = false)
    {
        if (imageFilePaths.IsNullOrEmpty())
        {
            return;
        }

        var query = (await comicImageRepository.GetQueryableAsync())
            .Where(x => x.LibraryPathId == libraryPath.Id)
            .Select(x => new
            {
                Id = x.Id,
                Path = x.Path,
                MediumId = x.ComicId
            });

        var repImages = await AsyncExecuter.ToListAsync(query);
        var comicId = repImages.FirstOrDefault()?.MediumId ?? Guid.Empty;

        if (repImages.IsNullOrEmpty())
        {
            // create comic
            await using var fileStream = File.OpenRead(imageFilePaths.First());
            var cover = await coverSaver.SaveAsync(fileStream);

            var title = Path.GetFileName(libraryPath.Path);
            var comic = new Comic(GuidGenerator.Create(), title, cover, libraryPath.LibraryId);
            await comicRepository.InsertAsync(comic, autoSave);
            comicId = comic.Id;
        }


        var deleteImages = repImages.Where(x => !imageFilePaths.Contains(x.Path)).ToList();

        if (!deleteImages.IsNullOrEmpty())
        {
            await comicImageRepository.DeleteManyAsync(deleteImages.Select(x => x.Id), autoSave);
            if (deleteImages.Count == repImages.Count)
            {
                await comicRepository.DeleteAsync(comicId, autoSave);
            }
        }

        var addImageFileInfos = imageFilePaths.Where(x => !repImages.Select(image => image.Path).Contains(x))
            .Select(x => new FileInfo(x)).ToList();

        var addImageFileEntities = addImageFileInfos.Select(addImage =>
            new ComicImage(GuidGenerator.Create(), addImage.Name, addImage.FullName, addImage.Extension,
                addImage.Length, comicId, libraryPath.LibraryId, libraryPath.Id)).ToList();

        if (!addImageFileEntities.IsNullOrEmpty())
        {
            await comicImageRepository.InsertManyAsync(addImageFileEntities, autoSave);
        }
    }

    public async Task ScanImageByArchiveInfoAsync(ArchiveInfo archiveInfo, Guid libraryId)
    {
        var archivePathIds = archiveInfo.Paths.Select(x => x.Id).ToList();

        var query = (await comicImageRepository.GetQueryableAsync()).Where(x =>
            x.ArchiveInfoPathId.HasValue && archivePathIds.Contains(x.ArchiveInfoPathId.Value));

        var repImages = await AsyncExecuter.ToListAsync(query);

        using var archive = ArchiveFactory.Open(archiveInfo.Path);
        var archiveEntries = archive.Entries.Where(x => !x.IsDirectory).ToList();
        // 仅处理图片文件
        var imageEntries = archiveEntries
            .Where(e =>
            {
                var key = e.Key ?? string.Empty;
                var ext = Path.GetExtension(key);
                return !string.IsNullOrEmpty(ext) && ImageExtensions.Contains(ext.ToLowerInvariant());
            })
            .ToList();

        var deleteImages = repImages.Where(x => !imageEntries.Select(a => a.Key).Contains(x.Path)).ToList();

        await comicImageRepository.DeleteManyAsync(deleteImages.Select(x => x.Id), true);

        var addImages = imageEntries.Where(x => !repImages.Select(a => a.Path).Contains(x.Key)).ToList();

        // 工具函数：标准化归档内目录key，使用'/'并保留末尾'/'
        static string? GetDirKeyFromEntryKey(string entryKey)
        {
            var key = entryKey.Replace('\\', '/');
            var lastSlash = key.LastIndexOf('/');
            if (lastSlash < 0)
            {
                return null; // 根目录文件
            }
            // 包含末尾'/'，以匹配大多数归档目录条目格式
            return key.Substring(0, lastSlash + 1);
        }

        var addImageEntities = new List<ComicImage>(addImages.Count);

        foreach (var addImage in addImages)
        {
            var addKey = addImage.Key ?? string.Empty;
            if (string.IsNullOrWhiteSpace(addKey))
            {
                continue; // 跳过无效条目
            }
            var dirKey = GetDirKeyFromEntryKey(addKey);

            Guid archiveInfoPathId;
            if (dirKey is null)
            {
                // 根目录
                var rootPath = archiveInfo.Paths.FirstOrDefault(x => x.Path == null);
                if (rootPath is null)
                {
                    rootPath = new ArchiveInfoPath(GuidGenerator.Create(), null, archiveInfo.Id);
                    archiveInfo.Paths.Add(rootPath);
                }
                archiveInfoPathId = rootPath.Id;
            }
            else
            {
                var dirPath = archiveInfo.Paths.FirstOrDefault(x => x.Path == dirKey);
                if (dirPath is null)
                {
                    // 归档中可能没有显式的目录条目，这里按需创建
                    dirPath = new ArchiveInfoPath(GuidGenerator.Create(), dirKey, archiveInfo.Id);
                    archiveInfo.Paths.Add(dirPath);
                }
                archiveInfoPathId = dirPath.Id;
            }

            addImageEntities.Add(new ComicImage(
                GuidGenerator.Create(),
                Path.GetFileName(addKey),
                addKey,
                Path.GetExtension(addKey) ?? string.Empty,
                addImage.Size,
                Guid.Empty,
                libraryId,
                archiveInfoPathId: archiveInfoPathId));
        }

        List<Comic> comics = [];

        foreach (var imageFilese in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
        {
            var mediumId = repImages.FirstOrDefault(x => Path.GetDirectoryName(x.Path) == imageFilese.Key)
                ?.ComicId;

            if (mediumId == null)
            {
                var title = Path.GetFileName(imageFilese.Key);
                if (title.IsNullOrEmpty())
                {
                    title = Path.GetFileNameWithoutExtension(archiveInfo.Path);
                }

                var firstPath = imageFilese.First().Path;
                var entry = imageEntries.FirstOrDefault(x => x.Key == firstPath)
                            ?? archiveEntries.FirstOrDefault(x => x.Key == firstPath);
                if (entry is null)
                {
                    // 找不到封面条目，跳过该分组
                    continue;
                }

                await using var entitySteam = entry.OpenEntryStream();
                var cover = await coverSaver.SaveAsync(entitySteam);
                var comic = new Comic(GuidGenerator.Create(), title, cover, libraryId);
                comics.Add(comic);

                mediumId = comic.Id;
            }

            foreach (var imageFile in imageFilese)
            {
                imageFile.ComicId = mediumId.Value;
            }
        }

        if (!comics.IsNullOrEmpty())
        {
            await comicRepository.InsertManyAsync(comics, true);
        }

        if (!addImageEntities.IsNullOrEmpty())
        {
            await comicImageRepository.InsertManyAsync(addImageEntities, true);
        }

        archiveInfo.LastResolveTime = DateTime.UtcNow;

        await archiveInfoRepository.UpdateAsync(archiveInfo, true);
    }
}