namespace Vctoon.Libraries;

public class Favorite : AggregateRoot<Guid>
{
    protected Favorite()
    {
    }

    public Favorite(
        Guid id,
        string name,
        string coverPath
    ) : base(id)
    {
        Name = name;
        CoverPath = coverPath;
    }

    public string Name { get; set; }
    public string CoverPath { get; set; }

    public List<ComicChapterFavorite> ComicChapterFavorites { get; set; } = new();
    public List<ComicFavorite> ComicFavorites { get; set; } = new();
    public List<ComicFileFavorite> ComicFileFavorites { get; set; } = new();
}