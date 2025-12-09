using Vctoon.Identities;

namespace Vctoon.Mediums;

public class Medium : AuditedAggregateRoot<Guid>
{
    protected Medium()
    {
    }

    public Medium(
        Guid id,
        MediumType mediumType,
        string title,
        string cover,
        Guid libraryId,
        Guid? libraryPathId = null,
        string description = ""
    ) : base(id)
    {
        MediumType = mediumType;
        Title = title;
        Cover = cover;
        LibraryId = libraryId;
        LibraryPathId = libraryPathId;
        Description = description;
    }

    public string Title { get; set; }

    public MediumType MediumType { get; set; }

    public string Description { get; set; }

    public string Cover { get; set; }

    public int ReadCount { get; set; }

    public Guid LibraryId { get; set; }

    public Guid? LibraryPathId { get; set; }

    public VideoDetail? VideoDetail { get; set; }

    public virtual List<Tag> Tags { get; } = new();

    public virtual List<Artist> Artists { get; set; } = new();

    public ICollection<IdentityUserReadingProcess> Processes { get; set; } = [];
}