namespace Vctoon.Libraries;

public class TagGroup : AggregateRoot<Guid>
{
    protected TagGroup()
    {
    }

    public TagGroup(
        Guid id,
        string name
    ) : base(id)
    {
        Name = name;
    }

    public string Name { get; set; }
    public List<Tag> Tags { get; set; } = new();
}