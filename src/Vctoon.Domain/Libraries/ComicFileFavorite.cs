namespace Vctoon.Libraries;

public class ComicFileFavorite : Entity<Guid>
{
    public string FileName { get; set; }

    public LibraryPath LibraryPath { get; set; }
    public Guid LibraryPathId { get; set; }

    public Guid FavoriteId { get; set; }
    public Favorite Favorite { get; set; }
}