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

    public virtual async Task ScanAsync(LibraryPath libraryPath, MediumType mediaType)
    {
        if (mediaType != MediumType.Video)
        {
            return;
        }

        await SendLibraryScanMessageAsync(libraryPath.LibraryId, L["ScanningLibraryPathDirectory"],
            libraryPath.Path);

        var videoFilePaths = Directory.GetFiles(libraryPath.Path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(x => VideoExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();

        if (videoFilePaths.IsNullOrEmpty())
        {
            return;
        }

        var videos = await videoRepository.GetListAsync(x => x.LibraryId == libraryPath.LibraryId);

        var deleteVideos = videos.Where(x => !videoFilePaths.Contains(x!.Path)).ToList();

        if (!deleteVideos.IsNullOrEmpty())
        {
            await videoRepository.DeleteManyAsync(deleteVideos.Select(x => x.Id), true);
        }


        var addVideos = new List<Video>();

        foreach (var videoFilePath in videoFilePaths.Where(path => videos.All(x => x!.Path != path)))
        {
            // video.Framerate = videoStream.FrameRate;
            // video.Codec = videoStream.CodecName;
            // video.Width = videoStream.Width;
            // video.Height = videoStream.Height;
            // video.Bitrate = videoStream.BitRate;
            // video.Duration = mediaAnalysis.Duration;
            // video.Ratio = @$"{videoStream.DisplayAspectRatio.Width}:{videoStream.DisplayAspectRatio.Height}";
            try
            {
                var mediaInfo = await FFProbe.AnalyseAsync(videoFilePath);
                var videoStream = mediaInfo.PrimaryVideoStream;
                var name = Path.GetFileNameWithoutExtension(videoFilePath);
                await using var coverSteam = await FFmpegHelper.GetCoverAsync(videoFilePath);
                var coverPath = await coverSaver.SaveAsync(coverSteam);
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
                    @$"{videoStream.DisplayAspectRatio.Width}:{videoStream.DisplayAspectRatio.Height}");
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
    }
}