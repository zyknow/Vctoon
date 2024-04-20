namespace Vctoon.Libraries;

public class Tag : AggregateRoot<Guid>
{
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

    public string Name { get; set; }
}