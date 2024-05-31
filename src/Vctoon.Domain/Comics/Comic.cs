using Vctoon.Libraries;

namespace Vctoon.Comics;

public class Comic : AuditedEntity<Guid>
{
    private readonly Guid _libraryCollectionId;
    
    protected Comic()
    {
    }
    
    public Comic(
        Guid id,
        string title,
        string coverPath,
        Guid libraryId
    ) : base(id)
    {
        Title = title;
        CoverPath = coverPath;
        LibraryId = libraryId;
    }
    
    public string Title { get; set; }
    
    public string CoverPath { get; set; }
    
    public Guid LibraryId { get; set; }
    
    public List<Tag> Tags { get; set; } = new();
    
    public List<ComicCollection> ComicCollections { get; set; } = new();
    
    public List<Artist> Artists { get; set; } = new();
    
    public List<Star> Stars { get; set; } = new();
    
    public List<Comment> Comments { get; set; } = new();
    
    public List<ImageFile> Images { get; set; } = new();
    
    public List<ContentProgress> Progresses { get; set; } = new();
}