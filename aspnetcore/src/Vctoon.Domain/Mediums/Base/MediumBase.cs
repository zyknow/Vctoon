using Vctoon.Identities;

namespace Vctoon.Mediums.Base;

[Serializable]
public abstract class MediumBase : AuditedAggregateRoot<Guid>, IMediumHasReadingProcess
{
    protected MediumBase()
    {
    }

    public MediumBase(Guid id,
        string title,
        string cover,
        Guid libraryId,
        Guid libraryPathId,
        string description = "") : base(id)
    {
        Title = title;
        Cover = cover;
        LibraryId = libraryId;
        Description = description;
        LibraryPathId = libraryPathId;
    }

    public string Title { get; set; }


    public string Description { get; set; }

    public string Cover { get; set; }

    public int ReadCount { get; set; }

    public Guid LibraryId { get; set; }

    public Guid LibraryPathId { get; set; }
    
    public virtual List<Tag> Tags { get; } = new();

    public virtual List<Artist> Artists { get; set; } = new();

    public ICollection<IdentityUserReadingProcess> Processes { get; set; } = [];
}