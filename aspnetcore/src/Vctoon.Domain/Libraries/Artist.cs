namespace Vctoon.Libraries;

public class Artist : Entity<Guid>
{
    protected Artist()
    {
    }

    public Artist(
        Guid id,
        string name
    ) : base(id)
    {
        Name = name;
    }

    public string Name { get; set; }
}