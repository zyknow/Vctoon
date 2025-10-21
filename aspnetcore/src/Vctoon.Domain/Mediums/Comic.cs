using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public class Comic : MediumBase
{
    protected Comic()
    {
    }

    public Comic(Guid id, string title, string cover, Guid libraryId, Guid libraryPathId,
        string description = "") : base(id,
        title, cover,
        libraryId, libraryPathId, description)
    {
    }

    public List<ComicImage> ComicImages { get; set; } = new();
}