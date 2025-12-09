using System.Globalization;
using SharpCompress.Archives;
using Vctoon.Mediums;

namespace Vctoon.Handlers;

public class ComicScanHandler(
    IComicImageRepository comicImageRepository,
    IArchiveInfoRepository archiveInfoRepository,
    CoverSaver coverSaver,
    IMediumRepository mediumRepository,
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


    public virtual async Task<ScanResult?> ScanAsync(LibraryPath libraryPath, MediumType mediaType)
    {
        if (mediaType != MediumType.Comic)
        {
            return null;
        }

        await SendLibraryScanMessageAsync(libraryPath.LibraryId, L["ScanningLibraryPathDirectory"],
            libraryPath.Path);

        var filePaths = Directory.GetFiles(libraryPath.Path, "*.*", SearchOption.TopDirectoryOnly);

        var imageFilePaths =
            filePaths.Where(x => ImageExtensions.Contains(Path.GetExtension(x).ToLowerInvariant())).ToList();

        var totalChanges = new ScanChanges();

        // 1) 目录中的纯图片
        var dirImageChanges = await ScanImageByLibraryPathAsync(libraryPath, imageFilePaths);
        totalChanges.Merge(dirImageChanges);

        // 2) 类归档文件（压缩包/PDF/EPUB）
        var archiveLikeFilePaths = filePaths
            .Where(x =>
            {
                var extension = Path.GetExtension(x);
                return ArchiveExtensions.Contains(extension) || PdfExtensions.Contains(extension) ||
                       EpubExtensions.Contains(extension);
            })
            .ToList();

        // 计算需要删除的 ArchiveInfo（统一到 changes）
        var deleteArchiveIds =
            await AsyncExecuter.ToListAsync(
                (await archiveInfoRepository.GetQueryableAsync())
                .Where(x => x.LibraryPathId == libraryPath.Id)
                .Where(x => !archiveLikeFilePaths.Contains(x.Path))
                .Select(x => x.Id));

        if (!deleteArchiveIds.IsNullOrEmpty())
        {
            totalChanges.ArchiveInfoIdsToDelete.AddRange(deleteArchiveIds);
        }

        foreach (var archiveFilePath in archiveLikeFilePaths)
        {
            var extension = Path.GetExtension(archiveFilePath).ToLowerInvariant();

            if (ArchiveExtensions.Contains(extension))
            {
                var (archiveInfo, aiChanges) =
                    await ScanArchiveInfoDirectoryStructureAsync(archiveFilePath, libraryPath.Id);

                totalChanges.Merge(aiChanges);

                var archiveImageChanges = await ScanImageByArchiveInfoAsync(archiveInfo, libraryPath.LibraryId);
                totalChanges.Merge(archiveImageChanges);
                continue;
            }

            var (documentInfo, ensureChanges) = await EnsureArchiveInfoAsync(archiveFilePath, libraryPath.Id);
            totalChanges.Merge(ensureChanges);

            if (PdfExtensions.Contains(extension))
            {
                var pdfChanges = await ScanPdfAsync(documentInfo, libraryPath.LibraryId);
                totalChanges.Merge(pdfChanges);
            }
            else if (EpubExtensions.Contains(extension))
            {
                var epubChanges = await ScanEpubAsync(documentInfo, libraryPath.LibraryId);
                totalChanges.Merge(epubChanges);
            }
        }

        // 统一提交所有变更（尽量保持原有顺序语义）
        // 删除
        if (totalChanges.ImageIdsToDelete.Count > 0)
        {
            await comicImageRepository.DeleteManyAsync(totalChanges.ImageIdsToDelete);
        }

        if (totalChanges.MediumIdsToDelete.Count > 0)
        {
            await mediumRepository.DeleteManyAsync(totalChanges.MediumIdsToDelete);
        }

        if (totalChanges.ArchiveInfoIdsToDelete.Count > 0)
        {
            await archiveInfoRepository.DeleteManyAsync(totalChanges.ArchiveInfoIdsToDelete);
        }

        // 插入/更新
        if (totalChanges.ArchiveInfosToInsert.Count > 0)
        {
            await archiveInfoRepository.InsertManyAsync(totalChanges.ArchiveInfosToInsert);
        }

        foreach (var ai in totalChanges.ArchiveInfosToUpdate)
        {
            await archiveInfoRepository.UpdateAsync(ai);
        }

        if (totalChanges.MediumsToInsert.Count > 0)
        {
            await mediumRepository.InsertManyAsync(totalChanges.MediumsToInsert);
        }

        if (totalChanges.ImagesToInsert.Count > 0)
        {
            await comicImageRepository.InsertManyAsync(totalChanges.ImagesToInsert);
        }

        var deletedCount = totalChanges.ImageIdsToDelete.Count
                           + totalChanges.MediumIdsToDelete.Count
                           + totalChanges.ArchiveInfoIdsToDelete.Count;
        var updatedCount = totalChanges.ArchiveInfosToUpdate.Count;
        var addedCount = totalChanges.ArchiveInfosToInsert.Count
                         + totalChanges.MediumsToInsert.Count
                         + totalChanges.ImagesToInsert.Count;

        if (deletedCount == 0 && updatedCount == 0 && addedCount == 0)
        {
            return null;
        }

        return new ScanResult(deletedCount, updatedCount, addedCount);
    }

    protected virtual async Task<(ArchiveInfo archiveInfo, ScanChanges changes)> ScanArchiveInfoDirectoryStructureAsync(
        string archivePath,
        Guid libraryPathId)
    {
        var changes = new ScanChanges();
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
            // 推迟保存
            changes.ArchiveInfosToInsert.Add(archiveInfo);
        }

        if (!NeedScanArchiveInfo(archiveInfo))
        {
            return (archiveInfo, changes);
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

        archiveInfo.LastResolveTime = DateTime.UtcNow;
        changes.ArchiveInfosToUpdate.Add(archiveInfo);

        return (archiveInfo, changes);
    }

    protected virtual bool NeedScanArchiveInfo(ArchiveInfo archiveInfo)
    {
        return archiveInfo.LastResolveTime is null ||
               File.GetLastWriteTimeUtc(archiveInfo.Path) > archiveInfo.LastResolveTime;
    }

    protected virtual async Task<(ArchiveInfo archiveInfo, ScanChanges changes)> EnsureArchiveInfoAsync(
        string documentPath, Guid libraryPathId)
    {
        var changes = new ScanChanges();
        var query = await archiveInfoRepository.WithDetailsAsync(x => x.Paths);
        var archiveInfo = await AsyncExecuter.FirstOrDefaultAsync(query,
            x => x.Path == documentPath && x.LibraryPathId == libraryPathId);

        var fileInfo = new FileInfo(documentPath);

        if (archiveInfo is null)
        {
            archiveInfo = new ArchiveInfo(GuidGenerator.Create(), documentPath, fileInfo.Length, libraryPathId,
                fileInfo.LastWriteTimeUtc, null);
            archiveInfo.Paths.Add(new ArchiveInfoPath(GuidGenerator.Create(), null, archiveInfo.Id));
            changes.ArchiveInfosToInsert.Add(archiveInfo);
        }
        else
        {
            archiveInfo.Size = fileInfo.Length;
            archiveInfo.LastModifyTime = fileInfo.LastWriteTimeUtc;

            if (archiveInfo.Paths.All(x => x.Path != null))
            {
                archiveInfo.Paths.Add(new ArchiveInfoPath(GuidGenerator.Create(), null, archiveInfo.Id));
            }

            changes.ArchiveInfosToUpdate.Add(archiveInfo);
        }

        return (archiveInfo, changes);
    }

    protected virtual async Task<ScanChanges> ScanPdfAsync(ArchiveInfo archiveInfo, Guid libraryId)
    {
        var changes = new ScanChanges();
        var archivePaths =
            archiveInfo.Paths.ToDictionary(x => x.Path ?? string.Empty, StringComparer.OrdinalIgnoreCase);
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
            changes.ImageIdsToDelete.AddRange(deleteImages.Select(x => x.Id));
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

        var newMediums = new List<Medium>();

        if (!addImageEntities.IsNullOrEmpty())
        {
            foreach (var group in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
            {
                var mediumId = existingImages
                    .FirstOrDefault(x => Path.GetDirectoryName(x.Path) == group.Key)
                    ?.MediumId;

                if (mediumId == null || mediumId == Guid.Empty)
                {
                    var title = Path.GetFileNameWithoutExtension(archiveInfo.Path) ?? archiveInfo.Path;
                    await using var coverStream = await documentContentService.RenderPdfPageAsync(archiveInfo.Path, 0)
                        .ConfigureAwait(false);
                    var cover = await coverSaver.SaveAsync(coverStream).ConfigureAwait(false);
                    var medium = new Medium(GuidGenerator.Create(), MediumType.Comic, title, cover, libraryId,
                        archiveInfo.LibraryPathId);
                    medium.MediumType = MediumType.Comic;
                    newMediums.Add(medium);
                    mediumId = medium.Id;
                }

                foreach (var imageFile in group)
                {
                    imageFile.MediumId = mediumId.Value;
                }
            }

            if (!newMediums.IsNullOrEmpty())
            {
                changes.MediumsToInsert.AddRange(newMediums);
            }

            changes.ImagesToInsert.AddRange(addImageEntities);
        }

        archiveInfo.LastResolveTime = DateTime.UtcNow;
        // 标记更新（LastResolveTime）
        changes.ArchiveInfosToUpdate.Add(archiveInfo);
        return changes;
    }

    protected virtual async Task<ScanChanges> ScanEpubAsync(ArchiveInfo archiveInfo, Guid libraryId)
    {
        var changes = new ScanChanges();
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
            changes.ImageIdsToDelete.AddRange(deleteImages.Select(x => x.Id));
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

        var newMediums = new List<Medium>();

        if (!addImageEntities.IsNullOrEmpty())
        {
            foreach (var group in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
            {
                var mediumId = existingImages
                    .FirstOrDefault(x => Path.GetDirectoryName(x.Path) == group.Key)
                    ?.MediumId;

                if (mediumId == null || mediumId == Guid.Empty)
                {
                    var title = Path.GetFileNameWithoutExtension(archiveInfo.Path) ?? archiveInfo.Path;
                    var coverDescriptor = manifestLookup.GetValueOrDefault(group.First().Path) ?? manifest.First();
                    await using var coverStream = await documentContentService
                        .GetEpubImageStreamAsync(archiveInfo.Path, coverDescriptor.Path)
                        .ConfigureAwait(false);
                    var cover = await coverSaver.SaveAsync(coverStream).ConfigureAwait(false);
                    var medium = new Medium(GuidGenerator.Create(),MediumType.Comic, title, cover, libraryId, archiveInfo.LibraryPathId);
                    medium.MediumType = MediumType.Comic;
                    newMediums.Add(medium);
                    mediumId = medium.Id;
                }

                foreach (var image in group)
                {
                    image.MediumId = mediumId.Value;
                }
            }

            if (!newMediums.IsNullOrEmpty())
            {
                changes.MediumsToInsert.AddRange(newMediums);
            }

            changes.ImagesToInsert.AddRange(addImageEntities);
        }

        archiveInfo.LastResolveTime = DateTime.UtcNow;
        changes.ArchiveInfosToUpdate.Add(archiveInfo);
        return changes;
    }

    private ArchiveInfoPath EnsureRootArchivePath(ArchiveInfo archiveInfo,
        IDictionary<string, ArchiveInfoPath>? cache = null)
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


    public async Task<ScanChanges> ScanImageByLibraryPathAsync(LibraryPath libraryPath, List<string> imageFilePaths)
    {
        var changes = new ScanChanges();
        if (imageFilePaths.IsNullOrEmpty())
        {
            return changes;
        }

        var query = (await comicImageRepository.GetQueryableAsync())
            .Where(x => x.LibraryPathId == libraryPath.Id)
            .Select(x => new
            {
                Id = x.Id,
                Path = x.Path,
                MediumId = x.MediumId
            });

        var repImages = await AsyncExecuter.ToListAsync(query);
        var mediumId = repImages.FirstOrDefault()?.MediumId ?? Guid.Empty;

        if (repImages.IsNullOrEmpty())
        {
            // create comic
            await using var fileStream = File.OpenRead(imageFilePaths.First());
            var cover = await coverSaver.SaveAsync(fileStream);

            var title = Path.GetFileName(libraryPath.Path);
            var medium = new Medium(GuidGenerator.Create(),MediumType.Comic, title, cover, libraryPath.LibraryId, libraryPath.Id);
            medium.MediumType = MediumType.Comic;
            changes.MediumsToInsert.Add(medium);
            mediumId = medium.Id;
        }


        var deleteImages = repImages.Where(x => !imageFilePaths.Contains(x.Path)).ToList();

        if (!deleteImages.IsNullOrEmpty())
        {
            changes.ImageIdsToDelete.AddRange(deleteImages.Select(x => x.Id));
            if (deleteImages.Count == repImages.Count)
            {
                // 删除该文件夹下的所有图片时，同时删除漫画实体
                if (mediumId != Guid.Empty)
                {
                    changes.MediumIdsToDelete.Add(mediumId);
                }
            }
        }

        var addImageFileInfos = imageFilePaths.Where(x => !repImages.Select(image => image.Path).Contains(x))
            .Select(x => new FileInfo(x)).ToList();

        var addImageFileEntities = addImageFileInfos.Select(addImage =>
            new ComicImage(GuidGenerator.Create(), addImage.Name, addImage.FullName, addImage.Extension,
                addImage.Length, mediumId, libraryPath.LibraryId, libraryPath.Id)).ToList();

        if (!addImageFileEntities.IsNullOrEmpty())
        {
            changes.ImagesToInsert.AddRange(addImageFileEntities);
        }

        return changes;
    }

    public async Task<ScanChanges> ScanImageByArchiveInfoAsync(ArchiveInfo archiveInfo, Guid libraryId)
    {
        var changes = new ScanChanges();
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

        if (!deleteImages.IsNullOrEmpty())
        {
            changes.ImageIdsToDelete.AddRange(deleteImages.Select(x => x.Id));
        }

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

        List<Medium> mediums = [];

        foreach (var imageFilese in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
        {
            var mediumId = repImages.FirstOrDefault(x => Path.GetDirectoryName(x.Path) == imageFilese.Key)
                ?.MediumId;

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
                var medium = new Medium(GuidGenerator.Create(),MediumType.Comic, title, cover, libraryId, archiveInfo.LibraryPathId);
                medium.MediumType = MediumType.Comic;
                mediums.Add(medium);

                mediumId = medium.Id;
            }

            foreach (var imageFile in imageFilese)
            {
                imageFile.MediumId = mediumId.Value;
            }
        }

        if (!mediums.IsNullOrEmpty())
        {
            changes.MediumsToInsert.AddRange(mediums);
        }

        if (!addImageEntities.IsNullOrEmpty())
        {
            changes.ImagesToInsert.AddRange(addImageEntities);
        }

        archiveInfo.LastResolveTime = DateTime.UtcNow;
        changes.ArchiveInfosToUpdate.Add(archiveInfo);
        return changes;
    }

    // 变更聚合模型
    public sealed class ScanChanges
    {
        public List<ComicImage> ImagesToInsert { get; } = [];
        public List<Guid> ImageIdsToDelete { get; } = [];
        public List<Medium> MediumsToInsert { get; } = [];
        public List<Guid> MediumIdsToDelete { get; } = [];
        public List<ArchiveInfo> ArchiveInfosToInsert { get; } = [];
        public List<ArchiveInfo> ArchiveInfosToUpdate { get; } = [];
        public List<Guid> ArchiveInfoIdsToDelete { get; } = [];

        public void Merge(ScanChanges other)
        {
            if (other is null) return;
            ImagesToInsert.AddRange(other.ImagesToInsert);
            ImageIdsToDelete.AddRange(other.ImageIdsToDelete);
            MediumsToInsert.AddRange(other.MediumsToInsert);
            MediumIdsToDelete.AddRange(other.MediumIdsToDelete);
            ArchiveInfosToInsert.AddRange(other.ArchiveInfosToInsert);
            ArchiveInfosToUpdate.AddRange(other.ArchiveInfosToUpdate);
            ArchiveInfoIdsToDelete.AddRange(other.ArchiveInfoIdsToDelete);
        }
    }
}