namespace Vctoon.Libraries;

public class Library : AggregateRoot<Guid>
{
    public string Name { get;  set; }
    public virtual List<LibraryPath> Paths { get; internal set; } = new();
}