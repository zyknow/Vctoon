namespace Vctoon.Identities;

public class IdentityUserReadingProcess : Entity
{
    protected IdentityUserReadingProcess()
    {
    }

    public IdentityUserReadingProcess(
        Guid userId,
        Guid mediumId,
        double progress = 0,
        DateTime? lastReadTime = null
    )
    {
        UserId = userId;
        MediumId = mediumId;
        Progress = progress;
        LastReadTime = lastReadTime;
    }

    public Guid UserId { get; set; }
    public Guid MediumId { get; set; }
    public double Progress { get; set; }
    public DateTime? LastReadTime { get; set; }

    public override object?[] GetKeys()
    {
        return [UserId, MediumId];
    }
}