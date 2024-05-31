namespace Vctoon.Libraries;

public class ImageFileFavorCollection : AuditedEntity<Guid>
{
    public string Title { get; set; }
    public bool IsPublic { get; set; }
    public Guid FavorId { get; set; }
    public Guid UserId { get; set; }
    public List<ImageFile> ImageFiles { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
}