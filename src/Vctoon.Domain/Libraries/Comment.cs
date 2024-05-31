namespace Vctoon.Libraries;

public class Comment : AuditedEntity<Guid>
{
    public string Content { get; set; }
    public Guid UserId { get; set; }
    public Guid ComicId { get; set; }
}