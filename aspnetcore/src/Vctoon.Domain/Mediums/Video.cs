using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public class Video : MediumBase
{
    protected Video()
    {
    }

    public Video(Guid id, string path, string title, string cover, Guid libraryId, double framerate,
        string codec,
        int width, int height, long bitrate, TimeSpan duration, string ratio, string description = "") : base(id, title,
        cover, libraryId, description)
    {
        Framerate = framerate;
        Codec = codec;
        Width = width;
        Height = height;
        Bitrate = bitrate;
        Ratio = ratio;
        Duration = duration;
        Path = path;
    }

    public double Framerate { get; set; }
    public string Codec { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Bitrate { get; set; }
    public string Ratio { get; set; }
    public TimeSpan Duration { get; set; }
    public string Path { get; set; }
}