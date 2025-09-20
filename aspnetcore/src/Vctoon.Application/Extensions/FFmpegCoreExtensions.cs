using FFMpegCore;
using Vctoon.Mediums;

namespace Vctoon.Extensions;

public static class FFmpegCoreExtensions
{
    public static Video SetVideoInfoByFfmpeg(this IMediaAnalysis mediaAnalysis, Video video)
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


        return video;
    }
}