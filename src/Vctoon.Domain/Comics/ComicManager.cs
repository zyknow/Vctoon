using Vctoon.Libraries;

namespace Vctoon.Comics;

public class ComicManager(
    IContentProgressRepository contentProgressRepository,
    IComicRepository comicRepository,
    IComicChapterRepository comicChapterRepository) : DomainService
{
}