namespace Vctoon.Libraries;

public class Star : AuditedEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid? ComicId { get; set; }
    public Guid? ImageFileFavorId { get; set; }
    public double Rating { get; set; }
}