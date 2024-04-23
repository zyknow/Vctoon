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
        Guid? comicChapterId
    ) : base(id)
    {
        UserId = userId;
        ComicChapterId = comicChapterId;
        CompletionRate = completionRate;
    }

    public Guid UserId { get; set; }
    public Guid? ComicChapterId { get; set; }
    public double CompletionRate { get; set; }
}