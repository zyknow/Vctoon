namespace Vctoon.Mediums.Dtos.Base;

public interface IMediumHasReadingProcessDto
{
    public double? ReadingProgress { get; set; }
    public DateTime? ReadingLastTime { get; set; }
}