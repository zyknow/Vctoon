namespace Vctoon.Libraries;

public class TagGroup : AggregateRoot<Guid>
{
    public string Name { get; set; }
    public List<Tag> Tags { get; set; } = new();

    protected TagGroup()
    {
    }

    public TagGroup(
        Guid id,
        string name,
        List<Tag> tags
    ) : base(id)
    {
        Name = name;
        Tags = tags;
    }
}
