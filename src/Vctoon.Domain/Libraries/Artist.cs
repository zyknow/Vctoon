namespace Vctoon.Libraries;

public class Artist : AuditedEntity<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}