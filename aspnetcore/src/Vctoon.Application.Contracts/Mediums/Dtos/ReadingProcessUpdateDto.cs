namespace Vctoon.Mediums.Dtos;

public class ReadingProcessUpdateDto
{
    public Guid MediumId { get; set; }
    public double Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}