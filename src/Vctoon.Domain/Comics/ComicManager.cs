using Vctoon.Libraries;

namespace Vctoon.Comics;

public class ComicManager(
    IContentProgressRepository contentProgressRepository,
    IComicRepository comicRepository,
    IImageFileRepository imageFileRepository,
    IArchiveInfoRepository archiveInfoRepository,
    ILibraryPathRepository libraryPathRepository) : DomainService
{
}