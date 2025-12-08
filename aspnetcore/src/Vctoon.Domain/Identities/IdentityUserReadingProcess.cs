namespace Vctoon.Identities;

public class IdentityUserReadingProcess : Entity<Guid>
{
    protected IdentityUserReadingProcess()
    {
    }

    public IdentityUserReadingProcess(
        Guid id,
        Guid userId,
        Guid mediumId,
        double progress = 0,
        DateTime? lastReadTime = null
    ) : base(id)
    {
        UserId = userId;
        Progress = progress;
        LastReadTime = lastReadTime;
        MediumId = mediumId;
    }

    public Guid UserId { get; set; }
    public Guid MediumId { get; set; }
    public double Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}