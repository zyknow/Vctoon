namespace Vctoon.Mediums.Dtos;

public class ReadingProcessUpdateDto
{
    public MediumType MediumType { get; set; }
    public Guid MediumId { get; set; }
    public double Progress { get; set; }
    public DateTime? ReadingLastTime { get; set; }
}