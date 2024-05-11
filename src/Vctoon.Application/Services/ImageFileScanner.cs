﻿using Microsoft.AspNetCore.SignalR;
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
    ImageCoverSaver imageCoverSaver
) : VctoonHubServiceBase(libraryScanHub), ITransientDependency
{
    public async Task ScanByLibraryPathAsync(LibraryPath libraryPath, List<string> imageFilePaths)
    {
        if (imageFilePaths.IsNullOrEmpty())
        {
            return;
        }
        
        var query = (await imageFileRepository.GetQueryableAsync())
            .Where(x => x.LibraryPathId == libraryPath.Id);
        
        var repImages = await AsyncExecuter.ToListAsync(query);
        
        var comicChapterId = repImages.FirstOrDefault()?.ComicChapterId ?? Guid.Empty;
        
        if (repImages.IsNullOrEmpty())
        {
            // create comic and comicChapter
            
            var cover = await imageCoverSaver.SaveAsync(File.OpenRead(imageFilePaths.First()));
            
            var title = Path.GetFileName(Path.GetDirectoryName(libraryPath.Path));
            
            var comic = new Comic(GuidGenerator.Create(), title, cover, libraryPath.LibraryId);
            
            // 创建ComicChapter和Comic
            var comicChapter = new ComicChapter(GuidGenerator.Create(), title, cover,
                imageFilePaths.Count, 0, comic.Id);
            
            await comicChapterRepository.InsertAsync(comicChapter);
            await comicRepository.InsertAsync(comic);
            
            comicChapterId = comicChapter.Id;
        }
        
        
        var deleteImages = repImages.Where(x => !imageFilePaths.Contains(x.Path)).ToList();
        
        if (!deleteImages.IsNullOrEmpty())
        {
            await imageFileRepository.DeleteManyAsync(deleteImages.Select(x => x.Id));
        }
        
        var addImageFileInfos = imageFilePaths.Where(x => !repImages.Select(image => image.Path).Contains(x))
            .Select(x => new FileInfo(x)).ToList();
        
        List<ImageFile> addImageFileEntities = addImageFileInfos.Select(addImage =>
            new ImageFile(GuidGenerator.Create(), addImage.Name, addImage.FullName, addImage.Extension,
                addImage.Length, 0, 0, comicChapterId, libraryPath.Id)).ToList();
        
        if (!addImageFileEntities.IsNullOrEmpty())
        {
            await imageFileRepository.InsertManyAsync(addImageFileEntities);
        }
    }
    
    public async Task ScanByArchiveInfoAsync(ArchiveInfo archiveInfo, Guid libraryId)
    {
        var query = (await imageFileRepository.GetQueryableAsync()).Where(x => x.ArchiveInfoPathId == archiveInfo.Id);
        
        var repImages = await AsyncExecuter.ToListAsync(query);
        
        using var archive = ArchiveFactory.Open(archiveInfo.Path);
        var archiveEntries = archive.Entries.Where(x => !x.IsDirectory).ToList();
        
        var deleteImages = repImages.Where(x => !archiveEntries.Select(a => a.Key).Contains(x.Path)).ToList();
        
        await imageFileRepository.DeleteManyAsync(deleteImages.Select(x => x.Id));
        
        var addImages = archiveEntries.Where(x => !repImages.Select(a => a.Path).Contains(x.Key)).ToList();
        
        
        List<ImageFile> addImageEntities = addImages.Select(addImage =>
        {
            var isRootFile = !addImage.Key.Contains(Path.DirectorySeparatorChar);
            
            var archiveInfoPathId = isRootFile
                ? archiveInfo.Paths.First(x => x.Path == null).Id
                : archiveInfo.Paths.First(x => x.Path == Path.GetDirectoryName(addImage.Key)).Id;
            
            return new ImageFile(GuidGenerator.Create(), Path.GetFileName(addImage.Key), addImage.Key,
                Path.GetExtension(addImage.Key),
                addImage.Size, 0, 0, Guid.Empty, archiveInfoPathId: archiveInfoPathId);
        }).ToList();
        
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
                
                var cover = await imageCoverSaver.SaveAsync(entry.OpenEntryStream());
                
                var comic = new Comic(GuidGenerator.Create(), title, cover, libraryId);
                
                var comicChapter = new ComicChapter(GuidGenerator.Create(), title, cover,
                    imageFilese.Count(), 0, comic.Id);
                
                await comicChapterRepository.InsertAsync(comicChapter);
                await comicRepository.InsertAsync(comic);
                
                comicChapterId = comicChapter.Id;
            }
            
            foreach (var imageFile in imageFilese)
            {
                imageFile.ComicChapterId = comicChapterId.Value;
            }
        }
        
        if (!addImageEntities.IsNullOrEmpty())
        {
            await imageFileRepository.InsertManyAsync(addImageEntities);
        }
        
        archiveInfo.LastResolveTime = DateTime.UtcNow;
    }
}