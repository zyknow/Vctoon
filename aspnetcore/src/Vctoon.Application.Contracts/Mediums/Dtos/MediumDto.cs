namespace Vctoon.Mediums.Dtos;

public class MediumDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string Cover { get; set; }

    public int ReadCount { get; set; }

    public Guid LibraryId { get; set; }

    public virtual List<TagDto> Tags { get; } = new();

    public virtual List<ArtistDto> Artists { get; set; } = new();

    public double? ReadingProgress { get; set; }
    public DateTime? ReadingLastTime { get; set; }
    public VideoDetail? VideoDetail { get; set; }
}
