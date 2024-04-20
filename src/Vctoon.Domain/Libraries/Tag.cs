namespace Vctoon.Libraries;

public class Tag : AggregateRoot<Guid>
{
    public string Name { get; set; }

    protected Tag()
    {
    }

    public Tag(
        Guid id,
        string name
    ) : base(id)
    {
        Name = name;
    }
}
