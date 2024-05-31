namespace Vctoon.Libraries;

public class Share : AuditedEntity<Guid>
{
    public string Code { get; set; }
    public Guid? ImageFileFavorId { get; set; }
    public Guid? ComicId { get; set; }
    public DateTime EndDateTime { get; set; }
}