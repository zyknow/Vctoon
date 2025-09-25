using System.Threading;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace Vctoon.Helper;

public class FFmpegHelper
{
    public static async Task<Stream> GetCoverAsync(
        string inputPath,
        int maxWidth = 640,
        double preferTimeSeconds = 0,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
        {
            throw new FileNotFoundException("inputPath not exisit ", inputPath);
        }

        var fs = new FileInfo(inputPath);
        var ms = new MemoryStream();

        try
        {
            // 1) 尝试提取内嵌封面（转成 PNG）
            await FFMpegArguments
                .FromFileInput(fs, o => o.WithCustomArgument("-noautorotate"))
                .OutputToPipe(new StreamPipeSink(ms), o => o
                    .WithCustomArgument(
                        "-map 0:v:m:side_data=attached_pic? -frames:v 1 -an -sn -dn -f image2 -vcodec png"))
                .ProcessAsynchronously();

            if (ms.Length > 0)
            {
                ms.Position = 0;
                return ms;
            }
        }
        catch
        {
            ms.SetLength(0); // 清空，继续兜底
        }

        // 2) 没有内嵌封面 -> 从视频中截取一帧
        var snapshotTime = preferTimeSeconds > 0 ? TimeSpan.FromSeconds(preferTimeSeconds) : TimeSpan.FromSeconds(3);
        ms = new MemoryStream();

        await FFMpegArguments
            .FromFileInput(fs, o =>
            {
                o.WithCustomArgument("-hwaccel auto -noautorotate");
                o.Seek(snapshotTime); // 输入侧 seek，快
            })
            .OutputToPipe(new StreamPipeSink(ms), o =>
            {
                var args = "-an -sn -dn -frames:v 1 -vsync vfr -f image2 -vcodec png";
                if (maxWidth > 0)
                {
                    // 注意：在 filtergraph 表达式中，逗号需要转义为 \, 否则会被解析为过滤器分隔符
                    args += $" -vf scale=min({maxWidth}\\,iw):-2";
                }

                o.WithCustomArgument(args);
            })
            .ProcessAsynchronously();

        ms.Position = 0;
        return ms;
    }
}