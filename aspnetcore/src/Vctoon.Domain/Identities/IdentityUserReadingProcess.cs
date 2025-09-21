using Vctoon.Mediums;

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
        MediumType mediumType,
        double progress = 0,
        DateTime? lastReadTime = null
    ) : base(id)
    {
        UserId = userId;
        Progress = progress;
        LastReadTime = lastReadTime;

        switch (mediumType)
        {
            case MediumType.Video:
                VideoId = mediumId;
                break;
            case MediumType.Comic:
                ComicId = mediumId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mediumType), mediumType, null);
        }
    }

    public Guid UserId { get; set; }
    public Guid? VideoId { get; set; }
    public Guid? ComicId { get; set; }
    public double Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}