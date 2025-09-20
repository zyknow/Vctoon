using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public class Comic : MediumBase
{
    protected Comic()
    {
    }

    public Comic(Guid id, string title, string cover, Guid libraryId, string description = "") : base(id,
        title, cover,
        libraryId, description)
    {
    }

    public List<ComicImage> ComicImages { get; set; } = new();
}