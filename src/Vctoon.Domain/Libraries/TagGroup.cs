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
        SetName(name);
    }

    public string Name { get; protected set; }
    public List<Tag> Tags { get; set; } = new();

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public void AddTag(Tag tag)
    {
        if (Tags.Any(x => x.Id == tag.Id))
        {
            return;
        }

        Tags.Add(tag);
    }

    public void AddTags(List<Tag> tags)
    {
        var addTags = tags.DistinctBy(x => x.Name).ToList();
        foreach (var tag in addTags)
        {
            AddTag(tag);
        }
    }

    public void RemoveTag(string name)
    {
        var tag = Tags.FirstOrDefault(x => x.Name == name);
        if (tag == null)
        {
            return;
        }

        Tags.Remove(tag);
    }

    public void RemoveTags(List<string> names)
    {
        foreach (var name in names)
        {
            RemoveTag(name);
        }
    }

    public void RemoveTag(Guid id)
    {
        var tag = Tags.FirstOrDefault(x => x.Id == id);
        if (tag == null)
        {
            return;
        }

        Tags.Remove(tag);
    }

    public void RemoveTags(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            RemoveTag(id);
        }
    }
}