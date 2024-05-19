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
    IComicChapterRepository comicChapterRepository,
    IComicRepository comicRepository,
    IHubContext<LibraryScanHub> libraryScanHub,
    ImageCoverSaver imageCoverSaver,
    ComicManager comicManager
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
                ComicChapterId = x.ComicChapterId
            });
        
        var repImages = await AsyncExecuter.ToListAsync(query);
        
        ComicChapter? chapter = !repImages.IsNullOrEmpty()
            ? await comicChapterRepository.GetAsync(repImages.First().ComicChapterId)
            : null;
        var comicChapterId = repImages.FirstOrDefault()?.ComicChapterId ?? Guid.Empty;
        
        if (repImages.IsNullOrEmpty())
        {
            // create comic and comicChapter
            await using FileStream? fileStream = File.OpenRead(imageFilePaths.First());
            string? cover = await imageCoverSaver.SaveAsync(fileStream);
            
            string? title = Path.GetFileName(libraryPath.Path);
            var comic = new Comic(GuidGenerator.Create(), title, cover, libraryPath.LibraryId);
            
            // create ComicChapter and Comic
            chapter = new ComicChapter(GuidGenerator.Create(), title, cover,
                comic.Id);
            
            await comicChapterRepository.InsertAsync(chapter, autoSave);
            await comicRepository.InsertAsync(comic, autoSave);
            
            comicChapterId = chapter.Id;
        }
        
        
        var deleteImages = repImages.Where(x => !imageFilePaths.Contains(x.Path)).ToList();
        
        if (!deleteImages.IsNullOrEmpty())
        {
            await imageFileRepository.DeleteManyAsync(deleteImages.Select(x => x.Id), autoSave);
            if (deleteImages.Count == repImages.Count)
            {
                await comicChapterRepository.DeleteAsync(comicChapterId, autoSave);
            }
        }
        
        var addImageFileInfos = imageFilePaths.Where(x => !repImages.Select(image => image.Path).Contains(x))
            .Select(x => new FileInfo(x)).ToList();
        
        List<ImageFile> addImageFileEntities = addImageFileInfos.Select(addImage =>
            new ImageFile(GuidGenerator.Create(), addImage.Name, addImage.FullName, addImage.Extension,
                addImage.Length, comicChapterId, libraryPath.Id)).ToList();
        
        if (!addImageFileEntities.IsNullOrEmpty())
        {
            await imageFileRepository.InsertManyAsync(addImageFileEntities, autoSave);
        }
    }
    
    public async Task ScanByArchiveInfoAsync(ArchiveInfo archiveInfo, Guid libraryId, bool autoSave = false)
    {
        var query = (await imageFileRepository.GetQueryableAsync()).Where(x => x.ArchiveInfoPathId == archiveInfo.Id);
        
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
                addImage.Size, Guid.Empty, archiveInfoPathId: archiveInfoPathId);
        }).ToList();
        
        List<Comic> addComics = [];
        List<ComicChapter> addComicChapters = [];
        
        foreach (var imageFilese in addImageEntities.GroupBy(x => Path.GetDirectoryName(x.Path)))
        {
            var comicChapterId = repImages.FirstOrDefault(x => Path.GetDirectoryName(x.Path) == imageFilese.Key)
                ?.ComicChapterId;
            
            if (comicChapterId == null)
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
                
                var comicChapter = new ComicChapter(GuidGenerator.Create(), title, cover,
                    comic.Id);
                
                addComics.Add(comic);
                addComicChapters.Add(comicChapter);
                
                comicChapterId = comicChapter.Id;
            }
            
            foreach (var imageFile in imageFilese)
            {
                imageFile.ComicChapterId = comicChapterId.Value;
            }
        }
        
        if (!addComicChapters.IsNullOrEmpty())
        {
            await comicChapterRepository.InsertManyAsync(addComicChapters, autoSave);
        }
        
        if (!addComics.IsNullOrEmpty())
        {
            await comicRepository.InsertManyAsync(addComics, autoSave);
        }
        
        if (!addImageEntities.IsNullOrEmpty())
        {
            await imageFileRepository.InsertManyAsync(addImageEntities, autoSave);
        }
        
        archiveInfo.LastResolveTime = DateTime.UtcNow;
    }
}