namespace Vctoon.Mediums.Dtos.Base;

public abstract class MediumGetListInputBase : PagedAndSortedResultRequestDto
{
    public string? Title { get; protected set; }

    public string? Description { get; protected set; }

    public List<Guid>? Tags { get; set; }
    public List<Guid>? Artists { get; set; }

    public Guid? LibraryId { get; internal set; }
}