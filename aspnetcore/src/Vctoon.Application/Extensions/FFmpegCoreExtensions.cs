using FFMpegCore;
using Vctoon.Mediums;

namespace Vctoon.Extensions;

public static class FFmpegCoreExtensions
{
    public static Medium SetVideoInfoByFfmpeg(this IMediaAnalysis mediaAnalysis, Medium medium)
    {
        if (mediaAnalysis == null)
        {
            throw new ArgumentNullException(nameof(mediaAnalysis));
        }

        var videoStream = mediaAnalysis.VideoStreams.FirstOrDefault();

        if (videoStream == null)
        {
            throw new InvalidOperationException("No video stream found in media analysis.");
        }

        // Calculate aspect ratio. Prefer DisplayAspectRatio; fallback to Width/Height simplified via GCD.
        string ratio;
        var displayAspect = videoStream.DisplayAspectRatio;
        if (displayAspect.Width > 0 && displayAspect.Height > 0)
        {
            ratio = $"{displayAspect.Width}:{displayAspect.Height}";
        }
        else
        {
            int w = videoStream.Width;
            int h = videoStream.Height;
            if (w > 0 && h > 0)
            {
                int Gcd(int a, int b)
                {
                    while (b != 0)
                    {
                        (a, b) = (b, a % b);
                    }

                    return a;
                }

                var g = Gcd(w, h);
                ratio = $"{w / g}:{h / g}";
            }
            else
            {
                ratio = "0:0"; // Unable to determine
            }
        }

        medium.VideoDetail = new VideoDetail
        {
            Framerate = videoStream.FrameRate,
            Codec = videoStream.CodecName ?? string.Empty,
            Width = videoStream.Width,
            Height = videoStream.Height,
            Bitrate = videoStream.BitRate,
            Duration = mediaAnalysis.Duration,
            Ratio = ratio,
            Path = medium.VideoDetail?.Path ?? string.Empty // Preserve path if exists
        };

        return medium;
    }
}
