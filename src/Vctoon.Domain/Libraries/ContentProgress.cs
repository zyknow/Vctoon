namespace Vctoon.Libraries;

public class ContentProgress : Entity<Guid>
{
    protected ContentProgress()
    {
    }
    
    public ContentProgress(
        Guid id,
        Guid userId,
        double completionRate,
        Guid? comicId = null
    ) : base(id)
    {
        UserId = userId;
        CompletionRate = completionRate;
        ComicId = comicId;
    }
    
    public Guid UserId { get; set; }
    public Guid? ComicId { get; set; }
    public double CompletionRate { get; set; }
}