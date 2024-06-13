using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using Vctoon.Comics;
using Vctoon.Hubs;
using Vctoon.Libraries;
using Vctoon.Services.Base;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Services;

public class ImageFileScanner(
    ILogger<ImageFileScanner> logger,
    IImageFileRepository imageFileRepository,
    IComicRepository comicRepository,
    IHubContext<LibraryScanHub> libraryScanHub,
    ImageCoverSaver imageCoverSaver,
    ComicManager comicManager,
    IArchiveInfoRepository archiveInfoRepository
) : VctoonHubServiceBase(libraryScanHub), ITransientDependency
{
    public async Task ScanByLibraryPathAsync(LibraryPath libraryPath, List<string> imageFilePaths, bool autoSave = false)
    {
        if (imageFilePaths.IsNullOrEmpty())
        {
            return;
        }

        var query = (await imageFileRepository.GetQueryableAsync())
            .Where(x => x.LibraryPathId == libraryPath.Id)
            .Select(x => new
            {
                Id = x.Id,
                Path = x.Path,
                ComicId = x.ComicId
            });

        var repImages = await AsyncExecuter.ToListAsync(query);

        // Comic? comic = !repImages.IsNullOrEmpty()
        //     ? await comicRepository.GetAsync(repImages.First().ComicId)
        //     : null;
        Guid comicId = repImages.FirstOrDefault()?.ComicId ?? Guid.Empty;

        if (repImages.IsNullOrEmpty())
        {
            // create comic
            await using FileStream? fileStream = File.OpenRead(imageFilePaths.First());
            string? cover = await imageCoverSaver.SaveAsync(fileStream);

            string? title = Path.GetFileName(libraryPath.Path);
            var comic = new Comic(GuidGenerator.Create(), title, cover, libraryPath.LibraryId);

            // create and Comic
            comic = new Comic(GuidGenerator.Create(), title, cover,
                libraryPath.LibraryId);

            await comicRepository.InsertAsync(comic, autoSave);
            comicId = comic.Id;
        }


        var deleteImages = repImages.Where(x => !imageFilePaths.Contains(x.Path)).ToList();

        if (!deleteImages.IsNullOrEmpty())
        {
            await imageFileRepository.DeleteManyAsync(deleteImages.Select(x => x.Id), autoSave);
            if (deleteImages.Count == repImages.Count)
            {
                await comicRepository.DeleteAsync(comicId, autoSave);
            }
        }

        var addImageFileInfos = imageFilePaths.Where(x => !repImages.Select(image => image.Path).Contains(x))
            .Select(x => new FileInfo(x)).ToList();

        List<ImageFile> addImageFileEntities = addImageFileInfos.Select(addImage =>
            new ImageFile(GuidGenerator.Create(), addImage.Name, addImage.FullName, addImage.Extension,
                addImage.Length, comicId, libraryPath.LibraryId, libraryPath.Id)).ToList();

        if (!addImageFileEntities.IsNullOrEmpty())
        {
            await imageFileRepository.InsertManyAsync(addImageFileEntities, autoSave);
        }
    }

    public async Task ScanByArchiveInfoAsync(ArchiveInfo archiveInfo, Guid libraryId, bool autoSave = false)
    {
        List<Guid> archivePathIds = archiveInfo.Paths.Select(x => x.Id).ToList();

        IQueryable<ImageFile> query = (await imageFileRepository.GetQueryableAsync()).Where(x =>
            archivePathIds.Contains(x.ArchiveInfoPathId.Value));

        var repImages = await AsyncExecuter.ToListAsync(query);

        using var archive = ArchiveFactory.Open(archiveInfo.Path);
        var archiveEntries = archive.Entries.Where(x => !x.IsDirectory).ToList();

        var deleteImages = repImages.Where(x => !archiveEntries.Select(a => a.Key).Contains(x.Path)).ToList();

        await imageFileRepository.DeleteManyAsync(deleteImages.Select(x => x.Id), autoSave);

        var addImages = archiveEntries.Where(x => !repImages.Select(a => a.Path).Contains(x.Key)).ToList();


        List<ImageFile> addImageEntities = addImages.Select(addImage =>
        {
            var isRootFile = !addImage.Key.Contains(Path.DirectorySeparatorChar);

            var archiveInfoPathId = isRootFile
                ? archiveInfo.Paths.First(x => x.Path == null).Id
                : archiveInfo.Paths.First(x => x.Path == Path.GetDirectoryName(addImage.Key)).Id;

            return new ImageFile(GuidGenerator.Create(), Path.GetFileName(addImage.Key), addImage.Key,
                Path.GetExtension(addImage.Key),
                addImage.Size, Guid.Empty, libraryId, archiveInfoPathId: archiveInfoPathId);
        }).ToList();

        List<Comic> addComics = [];

        foreach (var imageFilese in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
        {
            Guid? comicId = repImages.FirstOrDefault(x => Path.GetDirectoryName(x.Path) == imageFilese.Key)
                ?.ComicId;

            if (comicId == null)
            {
                var title = Path.GetFileName(imageFilese.Key);
                if (title.IsNullOrEmpty())
                {
                    title = Path.GetFileNameWithoutExtension(archiveInfo.Path);
                }

                var entry = archiveEntries.FirstOrDefault(x => x.Key == imageFilese.First().Path);

                await using Stream? entitySteam = entry.OpenEntryStream();
                string? cover = await imageCoverSaver.SaveAsync(entitySteam);

                var comic = new Comic(GuidGenerator.Create(), title, cover, libraryId);

                addComics.Add(comic);

                comicId = comic.Id;
            }

            foreach (var imageFile in imageFilese)
            {
                imageFile.ComicId = comicId.Value;
            }
        }

        if (!addComics.IsNullOrEmpty())
            await comicRepository.InsertManyAsync(addComics, autoSave);

        if (!addImageEntities.IsNullOrEmpty())
            await imageFileRepository.InsertManyAsync(addImageEntities, autoSave);

        archiveInfo.LastResolveTime = DateTime.UtcNow;

        await archiveInfoRepository.UpdateAsync(archiveInfo, autoSave);
    }
}