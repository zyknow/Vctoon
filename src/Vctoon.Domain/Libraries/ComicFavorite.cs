using Vctoon.Comics;

namespace Vctoon.Libraries;

public class ComicFavorite : Entity<Guid>
{
    public Guid ComicId { get; set; }
    public Comic Comic { get; set; }

    public Guid FavoriteId { get; set; }
    public Favorite Favorite { get; set; }
}