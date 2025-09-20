namespace Vctoon.Libraries;

public class Artist : AuditedEntity<Guid>
{
    protected Artist()
    {
    }

    public Artist(
        Guid id,
        string name,
        long slug,
        string description
    ) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
    }

    public string Name { get; set; }
    public long Slug { get; set; }
    public string Description { get; set; }
}