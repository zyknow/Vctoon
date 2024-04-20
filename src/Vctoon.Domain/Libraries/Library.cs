namespace Vctoon.Libraries;

public class Library : AggregateRoot<Guid>
{
    protected Library()
    {
    }

    public Library(
        Guid id,
        string name
    ) : base(id)
    {
        Name = name;
    }

    public Library(
        Guid id,
        string name,
        List<LibraryPath> paths
    ) : base(id)
    {
        Name = name;
        Paths = paths;
    }

    public string Name { get; set; }
    public virtual List<LibraryPath> Paths { get; internal set; } = new();
}