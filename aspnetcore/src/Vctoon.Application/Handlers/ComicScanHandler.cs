using System.Globalization;
using SharpCompress.Archives;
using Vctoon.Mediums;
using Vctoon.Services;

namespace Vctoon.Handlers;

public class ComicScanHandler(
    IComicImageRepository comicImageRepository,
    IArchiveInfoRepository archiveInfoRepository,
    CoverSaver coverSaver,
    IComicRepository comicRepository,
    IDocumentContentService documentContentService)
    : VctoonService, IMediumScanHandler
{
    public readonly HashSet<string> ArchiveExtensions =
        new([".zip", ".rar", ".cbr", ".cbz", ".7z", ".cb7"], StringComparer.OrdinalIgnoreCase);

    public readonly HashSet<string> PdfExtensions = new([".pdf"], StringComparer.OrdinalIgnoreCase);
    public readonly HashSet<string> EpubExtensions = new([".epub"], StringComparer.OrdinalIgnoreCase);

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

        var archiveLikeFilePaths = filePaths
            .Where(x =>
            {
                var extension = Path.GetExtension(x);
                return ArchiveExtensions.Contains(extension) || PdfExtensions.Contains(extension) ||
                       EpubExtensions.Contains(extension);
            })
            .ToList();

        var deleteArchiveIds =
            await AsyncExecuter.ToListAsync(
                (await archiveInfoRepository.GetQueryableAsync())
                .Where(x => x.LibraryPathId == libraryPath.Id)
                .Where(x => !archiveLikeFilePaths.Contains(x.Path))
                .Select(x => x.Id));

        if (!deleteArchiveIds.IsNullOrEmpty())
        {
            await archiveInfoRepository.DeleteManyAsync(deleteArchiveIds, true);
        }

        foreach (var archiveFilePath in archiveLikeFilePaths)
        {
            var extension = Path.GetExtension(archiveFilePath).ToLowerInvariant();

            if (ArchiveExtensions.Contains(extension))
            {
                var archiveInfo =
                    await ScanArchiveInfoDirectoryStructureAsync(archiveFilePath, libraryPath.Id);

                await ScanImageByArchiveInfoAsync(archiveInfo, libraryPath.LibraryId);
                continue;
            }

            var documentInfo = await EnsureArchiveInfoAsync(archiveFilePath, libraryPath.Id);

            if (PdfExtensions.Contains(extension))
            {
                await ScanPdfAsync(documentInfo, libraryPath.LibraryId);
            }
            else if (EpubExtensions.Contains(extension))
            {
                await ScanEpubAsync(documentInfo, libraryPath.LibraryId);
            }
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

    protected virtual async Task<ArchiveInfo> EnsureArchiveInfoAsync(string documentPath, Guid libraryPathId)
    {
        var query = await archiveInfoRepository.WithDetailsAsync(x => x.Paths);
        var archiveInfo = await AsyncExecuter.FirstOrDefaultAsync(query,
            x => x.Path == documentPath && x.LibraryPathId == libraryPathId);

        var fileInfo = new FileInfo(documentPath);

        if (archiveInfo is null)
        {
            archiveInfo = new ArchiveInfo(GuidGenerator.Create(), documentPath, fileInfo.Length, libraryPathId,
                fileInfo.LastWriteTimeUtc, null);
            archiveInfo.Paths.Add(new ArchiveInfoPath(GuidGenerator.Create(), null, archiveInfo.Id));
            await archiveInfoRepository.InsertAsync(archiveInfo, true);
        }
        else
        {
            archiveInfo.Size = fileInfo.Length;
            archiveInfo.LastModifyTime = fileInfo.LastWriteTimeUtc;

            if (archiveInfo.Paths.All(x => x.Path != null))
            {
                archiveInfo.Paths.Add(new ArchiveInfoPath(GuidGenerator.Create(), null, archiveInfo.Id));
            }

            await archiveInfoRepository.UpdateAsync(archiveInfo, true);
        }

        return archiveInfo;
    }

    protected virtual async Task ScanPdfAsync(ArchiveInfo archiveInfo, Guid libraryId)
    {
        var archivePaths = archiveInfo.Paths.ToDictionary(x => x.Path ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        var rootPath = EnsureRootArchivePath(archiveInfo, archivePaths);

        var archivePathIds = archiveInfo.Paths.Select(x => x.Id).ToList();

        var existingQuery = (await comicImageRepository.GetQueryableAsync())
            .Where(x => x.ArchiveInfoPathId.HasValue && archivePathIds.Contains(x.ArchiveInfoPathId.Value));

        var existingImages = await AsyncExecuter.ToListAsync(existingQuery);
        var existingPaths = existingImages.Select(x => x.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var pageCount = await documentContentService.GetPdfPageCountAsync(archiveInfo.Path);

        var targetKeys = Enumerable.Range(0, pageCount).Select(FormatPdfPageKey).ToList();

        var deleteImages = existingImages.Where(x => !targetKeys.Contains(x.Path)).ToList();

        if (!deleteImages.IsNullOrEmpty())
        {
            await comicImageRepository.DeleteManyAsync(deleteImages.Select(x => x.Id), true);
        }

        var addImageEntities = new List<ComicImage>();

        foreach (var (key, pageIndex) in targetKeys.Select((value, idx) => (value, idx)))
        {
            if (existingPaths.Contains(key))
            {
                continue;
            }

            var name = $"Page {(pageIndex + 1).ToString("D4", CultureInfo.InvariantCulture)}";

            addImageEntities.Add(new ComicImage(
                GuidGenerator.Create(),
                name,
                key,
                ".png",
                0,
                Guid.Empty,
                libraryId,
                archiveInfoPathId: rootPath.Id));
        }

        var newComics = new List<Comic>();

        if (!addImageEntities.IsNullOrEmpty())
        {
            foreach (var group in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
            {
                var mediumId = existingImages
                    .FirstOrDefault(x => Path.GetDirectoryName(x.Path) == group.Key)
                    ?.ComicId;

                if (mediumId == null || mediumId == Guid.Empty)
                {
                    var title = Path.GetFileNameWithoutExtension(archiveInfo.Path) ?? archiveInfo.Path;
                    await using var coverStream = await documentContentService.RenderPdfPageAsync(archiveInfo.Path, 0)
                        .ConfigureAwait(false);
                    var cover = await coverSaver.SaveAsync(coverStream).ConfigureAwait(false);
                    var comic = new Comic(GuidGenerator.Create(), title, cover, libraryId);
                    newComics.Add(comic);
                    mediumId = comic.Id;
                }

                foreach (var imageFile in group)
                {
                    imageFile.ComicId = mediumId.Value;
                }
            }

            if (!newComics.IsNullOrEmpty())
            {
                await comicRepository.InsertManyAsync(newComics, true);
            }

            await comicImageRepository.InsertManyAsync(addImageEntities, true);
        }

        archiveInfo.LastResolveTime = DateTime.UtcNow;
        await archiveInfoRepository.UpdateAsync(archiveInfo, true);
    }

    protected virtual async Task ScanEpubAsync(ArchiveInfo archiveInfo, Guid libraryId)
    {
        var pathCache = archiveInfo.Paths.ToDictionary(x => x.Path ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        var rootPath = EnsureRootArchivePath(archiveInfo, pathCache);

        var archivePathIds = archiveInfo.Paths.Select(x => x.Id).ToList();

        var existingQuery = (await comicImageRepository.GetQueryableAsync())
            .Where(x => x.ArchiveInfoPathId.HasValue && archivePathIds.Contains(x.ArchiveInfoPathId.Value));

        var existingImages = await AsyncExecuter.ToListAsync(existingQuery);
        var existingPaths = existingImages.Select(x => x.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var manifest = await documentContentService.GetEpubImageManifestAsync(archiveInfo.Path);
        var manifestLookup = manifest.ToDictionary(x => x.Path, StringComparer.OrdinalIgnoreCase);

        var deleteImages = existingImages.Where(x => !manifestLookup.ContainsKey(x.Path)).ToList();

        if (!deleteImages.IsNullOrEmpty())
        {
            await comicImageRepository.DeleteManyAsync(deleteImages.Select(x => x.Id), true);
        }

        var addImageEntities = new List<ComicImage>();

        foreach (var descriptor in manifest)
        {
            if (existingPaths.Contains(descriptor.Path))
            {
                continue;
            }

            var directoryKey = GetDirectoryKey(descriptor.Path);
            var archivePath = directoryKey is null
                ? rootPath
                : GetOrAddArchivePath(archiveInfo, directoryKey, pathCache);

            var extension = ResolveImageExtension(descriptor.Path, descriptor.ContentType);
            var fileName = Path.GetFileName(descriptor.Path);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"Page {(descriptor.Index + 1).ToString("D4", CultureInfo.InvariantCulture)}{extension}";
            }

            addImageEntities.Add(new ComicImage(
                GuidGenerator.Create(),
                fileName,
                descriptor.Path,
                extension,
                0,
                Guid.Empty,
                libraryId,
                archiveInfoPathId: archivePath.Id));
        }

        var newComics = new List<Comic>();

        if (!addImageEntities.IsNullOrEmpty())
        {
            foreach (var group in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
            {
                var mediumId = existingImages
                    .FirstOrDefault(x => Path.GetDirectoryName(x.Path) == group.Key)
                    ?.ComicId;

                if (mediumId == null || mediumId == Guid.Empty)
                {
                    var title = Path.GetFileNameWithoutExtension(archiveInfo.Path) ?? archiveInfo.Path;
                    var coverDescriptor = manifestLookup.GetValueOrDefault(group.First().Path) ?? manifest.First();
                    await using var coverStream = await documentContentService
                        .GetEpubImageStreamAsync(archiveInfo.Path, coverDescriptor.Path)
                        .ConfigureAwait(false);
                    var cover = await coverSaver.SaveAsync(coverStream).ConfigureAwait(false);
                    var comic = new Comic(GuidGenerator.Create(), title, cover, libraryId);
                    newComics.Add(comic);
                    mediumId = comic.Id;
                }

                foreach (var image in group)
                {
                    image.ComicId = mediumId.Value;
                }
            }

            if (!newComics.IsNullOrEmpty())
            {
                await comicRepository.InsertManyAsync(newComics, true);
            }

            await comicImageRepository.InsertManyAsync(addImageEntities, true);
        }

        archiveInfo.LastResolveTime = DateTime.UtcNow;
        await archiveInfoRepository.UpdateAsync(archiveInfo, true);
    }

    private ArchiveInfoPath EnsureRootArchivePath(ArchiveInfo archiveInfo, IDictionary<string, ArchiveInfoPath>? cache = null)
    {
        var root = archiveInfo.Paths.FirstOrDefault(x => x.Path == null);

        if (root is null)
        {
            root = new ArchiveInfoPath(GuidGenerator.Create(), null, archiveInfo.Id);
            archiveInfo.Paths.Add(root);
        }

        cache?.TryAdd(string.Empty, root);
        return root;
    }

    private ArchiveInfoPath GetOrAddArchivePath(
        ArchiveInfo archiveInfo,
        string directoryKey,
        IDictionary<string, ArchiveInfoPath> cache)
    {
        directoryKey = directoryKey.Replace('\\', '/');

        if (cache.TryGetValue(directoryKey, out var existing))
        {
            return existing;
        }

        var archivePath = new ArchiveInfoPath(GuidGenerator.Create(), directoryKey, archiveInfo.Id);
        archiveInfo.Paths.Add(archivePath);
        cache[directoryKey] = archivePath;
        return archivePath;
    }

    private static string FormatPdfPageKey(int pageIndex)
    {
        return $"pdf-page-{pageIndex.ToString("D6", CultureInfo.InvariantCulture)}";
    }

    private static string? GetDirectoryKey(string entryKey)
    {
        var normalized = entryKey.Replace('\\', '/');
        var lastSlash = normalized.LastIndexOf('/');

        return lastSlash < 0 ? null : normalized[..(lastSlash + 1)];
    }

    private static string ResolveImageExtension(string path, string? contentType)
    {
        var extension = Path.GetExtension(path);

        if (!string.IsNullOrWhiteSpace(extension))
        {
            return extension;
        }

        return contentType?.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/svg+xml" => ".svg",
            _ => ".jpg"
        };
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

        var pathCache = archiveInfo.Paths.ToDictionary(x => x.Path ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        var rootPath = EnsureRootArchivePath(archiveInfo, pathCache);

        var addImageEntities = new List<ComicImage>(addImages.Count);

        foreach (var addImage in addImages)
        {
            var addKey = addImage.Key ?? string.Empty;
            if (string.IsNullOrWhiteSpace(addKey))
            {
                continue; // 跳过无效条目
            }
            var dirKey = GetDirectoryKey(addKey);
            var archiveInfoPath = dirKey is null
                ? rootPath
                : GetOrAddArchivePath(archiveInfo, dirKey, pathCache);

            addImageEntities.Add(new ComicImage(
                GuidGenerator.Create(),
                Path.GetFileName(addKey),
                addKey,
                Path.GetExtension(addKey) ?? string.Empty,
                addImage.Size,
                Guid.Empty,
                libraryId,
                archiveInfoPathId: archiveInfoPath.Id));
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