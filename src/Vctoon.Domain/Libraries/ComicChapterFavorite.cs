using Vctoon.Comics;

namespace Vctoon.Libraries;

public class ComicChapterFavorite : Entity<Guid>
{
    public Guid FavoriteId { get; set; }
    public Favorite Favorite { get; set; }

    public Guid ComicChapterId { get; set; }
    public ComicChapter ComicChapter { get; set; }
}