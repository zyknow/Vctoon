namespace Vctoon.Libraries;

public class Tag : AggregateRoot<Guid>
{
    public string Name { get; set; }
}