using FFMpegCore;
using Microsoft.Extensions.Logging;
using Vctoon.Helper;
using Vctoon.Mediums;

namespace Vctoon.Handlers;

public class VideoScanHandler(IVideoRepository videoRepository, CoverSaver coverSaver)
    : VctoonService, IMediumScanHandler
{
    public readonly List<string> VideoExtensions =
        [".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm"];

    public virtual async Task<ScanResult?> ScanAsync(LibraryPath libraryPath, MediumType mediaType)
    {
        if (mediaType != MediumType.Video)
        {
            return null;
        }

        await SendLibraryScanMessageAsync(libraryPath.LibraryId, L["ScanningLibraryPathDirectory"],
            libraryPath.Path);

        var videoFilePaths = Directory.GetFiles(libraryPath.Path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(x => VideoExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();

        if (videoFilePaths.IsNullOrEmpty())
        {
            return null;
        }

        var videos = await videoRepository.GetListAsync(x => x.LibraryId == libraryPath.LibraryId);

        var deleteVideos = videos.Where(x => x.LibraryPathId == libraryPath.Id && !videoFilePaths.Contains(x!.Path)).ToList();

        var deletedCount = 0;
        if (!deleteVideos.IsNullOrEmpty())
        {
            await videoRepository.DeleteManyAsync(deleteVideos.Select(x => x.Id), true);
            deletedCount = deleteVideos.Count;
        }


        var addVideos = new List<Video>();

        foreach (var videoFilePath in videoFilePaths.Where(path => videos.All(x => x!.Path != path)))
        {
            try
            {
                var mediaInfo = await FFProbe.AnalyseAsync(videoFilePath);
                var videoStream = mediaInfo.PrimaryVideoStream;
                var name = Path.GetFileNameWithoutExtension(videoFilePath);
                await using var coverSteam = await FFmpegHelper.GetCoverAsync(videoFilePath);
                var coverPath = await coverSaver.SaveAsync(coverSteam);

                // Calculate aspect ratio. Prefer DisplayAspectRatio; fallback to Width/Height simplified via GCD.
                string ratio;
                var displayAspect = videoStream?.DisplayAspectRatio ?? (0,0);
                if (displayAspect.Width > 0 && displayAspect.Height > 0)
                {
                    ratio = $"{displayAspect.Width}:{displayAspect.Height}";
                }
                else
                {
                    int w = videoStream?.Width ?? 0;
                    int h = videoStream?.Height ?? 0;
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

                var mediumInfo = new Video(
                    GuidGenerator.Create(),
                    videoFilePath,
                    name,
                    coverPath,
                    libraryPath.LibraryId,
                    libraryPath.Id,
                    videoStream!.FrameRate,
                    videoStream.CodecName ?? string.Empty,
                    videoStream.Width,
                    videoStream.Height,
                    videoStream.BitRate,
                    mediaInfo.Duration,
                    ratio);
                addVideos.Add(mediumInfo);
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to analyse video file: {VideoFilePath}", videoFilePath);
            }
        }

        if (addVideos.Any())
        {
            await videoRepository.InsertManyAsync(addVideos, true);
        }

        if (deletedCount == 0 && addVideos.Count == 0)
        {
            return null;
        }

        return new ScanResult(deletedCount, 0, addVideos.Count);
    }
}