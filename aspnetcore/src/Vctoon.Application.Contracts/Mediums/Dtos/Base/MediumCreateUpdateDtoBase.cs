namespace Vctoon.Mediums.Dtos.Base;

public abstract class MediumCreateUpdateDtoBase
{
    public string Title { get; protected set; }

    public string Description { get; protected set; }

    public string Cover { get; protected set; }

    public Guid LibraryId { get; internal set; }
}