using Vctoon.Mediums;

namespace Vctoon.Libraries;

public class Tag : AuditedEntity<Guid>
{
    protected Tag()
    {
    }

    public Tag(
        Guid id,
        string name
    ) : base(id)
    {
        SetName(name);
    }

    public string Name { get; protected set; }

    public virtual List<Medium> Mediums { get; set; } = new();

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }
}