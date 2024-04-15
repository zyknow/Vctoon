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

    public string Name { get; protected set; }
    public virtual List<LibraryPath> Paths { get; internal set; } = new();

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }
}