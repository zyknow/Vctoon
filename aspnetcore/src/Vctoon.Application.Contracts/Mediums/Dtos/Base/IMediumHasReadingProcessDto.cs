namespace Vctoon.Mediums.Dtos.Base;

public interface IMediumHasReadingProcessDto
{
    public double? Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}