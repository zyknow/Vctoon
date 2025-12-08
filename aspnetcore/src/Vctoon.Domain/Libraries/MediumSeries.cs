using Vctoon.Mediums;

namespace Vctoon.Libraries;

public class MediumCollection : AuditedAggregateRoot<Guid>
{
    protected MediumCollection()
    {
    }

    public MediumCollection(
        Guid id,
        string name,
        Guid libraryId
    ) : base(id)
    {
        Name = name;
        LibraryId = libraryId;
    }

    public string Name { get; set; }

    public Guid LibraryId { get; set; }

    public List<Medium> Mediums { get; set; } = [];
}
