using Vctoon.Comics;

namespace Vctoon.Libraries;

public class ComicCollection : AuditedEntity<Guid>
{
    public string Title { get; set; }
    public Guid LibraryId { get; set; }
    public List<Comic> Comics { get; set; } = new();
}