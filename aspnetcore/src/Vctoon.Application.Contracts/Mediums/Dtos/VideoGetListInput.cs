namespace Vctoon.Mediums.Dtos;

[Serializable]
public class VideoGetListInput : MediumGetListInputBase, IMediumHasReadingProcessDto
{
    public double? Framerate { get; set; }

    public string? Codec { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public long? Bitrate { get; set; }

    public string? Ratio { get; set; }
    public double? Progress { get; set; }
    public DateTime? LastReadTime { get; set; }
}