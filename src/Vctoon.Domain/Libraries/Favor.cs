using Vctoon.Comics;

namespace Vctoon.Libraries;

public class Favor : AuditedEntity<Guid>
{
    public Guid UserId { get; set; }
    public bool IsPublic { get; set; }
    public string Title { get; set; }
    public List<Comic> Comics { get; set; } = new();
    public List<ImageFileFavorCollection> ImageFilesFavors { get; set; } = new();
}