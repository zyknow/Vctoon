namespace Vctoon.Mediums.Dtos;

[Serializable]
public class VideoCreateUpdateDto : MediumCreateUpdateDtoBase
{
    public double Framerate { get; set; }

    public string Codec { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public long Bitrate { get; set; }

    public string Ratio { get; set; }

    public TimeSpan Duration { get; set; }
}