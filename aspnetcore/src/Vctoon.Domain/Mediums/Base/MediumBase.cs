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
        string description = "") : base(id)
    {
        Title = title;
        Cover = cover;
        LibraryId = libraryId;
        Description = description;
    }

    public string Title { get; set; }


    public string Description { get; set; }

    public string Cover { get; set; }

    public int ReadCount { get; set; }

    public Guid LibraryId { get; set; }
    public virtual List<Tag> Tags { get; } = new();

    public virtual List<Artist> Artists { get; set; } = new();

    public ICollection<IdentityUserReadingProcess> Processes { get; set; } = [];

    public virtual void AddTag(Tag tag)
    {
        Check.NotNull(tag, nameof(tag));

        if (Tags.Any(x => x.Name == tag.Name || x.Id == tag.Id))
        {
            throw new Exception(@$"{nameof(tag)} is already exist.");
        }

        Tags.Add(tag);
    }

    public virtual void RemoveTag(Tag tag)
    {
        Check.NotNull(tag, nameof(tag));
        Tags.RemoveAll(t => t.Name == tag.Name || t.Id == tag.Id);
    }
}