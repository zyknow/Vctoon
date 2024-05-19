using Vctoon.Libraries;

namespace Vctoon.Comics;

public class ComicManager(
    IContentProgressRepository contentProgressRepository,
    IComicRepository comicRepository,
    IComicChapterRepository comicChapterRepository,
    IImageFileRepository imageFileRepository,
    ILibraryPathRepository libraryPathRepository) : DomainService
{
    public async Task CheckRemoveEmptyComicAsync(Guid libraryId, bool autoSave)
    {
        await CheckRemoveEmptyComicImageAsync(libraryId, autoSave);
        
        IQueryable<Comic> query = await comicRepository.GetQueryableAsync();
        
        List<Guid> checkComicIds = await AsyncExecuter.ToListAsync(query
            .Where(x => x.LibraryId == libraryId)
            .Select(x => x.Id));
        
        await CheckRemoveEmptyComicChapterAsync(checkComicIds, autoSave);
        
        List<Guid> removeComicIds = await AsyncExecuter.ToListAsync(query
            .Where(x => checkComicIds.Contains(x.Id) && !x.Chapters.Any())
            .Select(x => x.Id));
        
        if (removeComicIds.IsNullOrEmpty())
        {
            return;
        }
        
        await comicRepository.DeleteManyAsync(removeComicIds, autoSave);
    }
    
    private async Task CheckRemoveEmptyComicChapterAsync(List<Guid> comicIds, bool autoSave)
    {
        IQueryable<ComicChapter> query = await comicChapterRepository.GetQueryableAsync();
        List<Guid> comicChapterIds = await AsyncExecuter.ToListAsync(query
            .Where(x => comicIds.Contains(x.ComicId) && !x.Images.Any())
            .Select(x => x.Id));
        
        if (comicChapterIds.IsNullOrEmpty())
        {
            return;
        }
        
        await comicChapterRepository.DeleteManyAsync(comicChapterIds, autoSave);
    }
    
    private async Task CheckRemoveEmptyComicImageAsync(Guid libraryId, bool autoSave)
    {
        List<Guid> libraryPathIds = await AsyncExecuter.ToListAsync((await libraryPathRepository.GetQueryableAsync())
            .Where(x => x.LibraryId == libraryId).Select(x => x.Id));
        IQueryable<ImageFile> query = await imageFileRepository.GetQueryableAsync();
        List<Guid> imageIds = await AsyncExecuter.ToListAsync(query
            .Where(x => x.LibraryPathId == null || !libraryPathIds.Contains(x.LibraryPathId.Value))
            .Select(x => x.Id));
        
        if (imageIds.IsNullOrEmpty())
        {
            return;
        }
        
        await imageFileRepository.DeleteManyAsync(imageIds, autoSave);
    }
}