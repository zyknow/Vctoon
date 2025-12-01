using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Vctoon.Helper;
using Vctoon.Identities;
using Vctoon.Mediums.Base;
using Vctoon.Mediums.Dtos;
using Vctoon.Permissions;
using Volo.Abp;
using Volo.Abp.Content;

namespace Vctoon.Mediums;

public class VideoAppService(IVideoRepository repository)
    : MediumBaseAppService<Video, VideoDto, VideoGetListOutputDto, VideoGetListInput,
            VideoCreateUpdateDto,
            VideoCreateUpdateDto>(repository),
        IVideoAppService
{
    protected override string? GetPolicyName { get; set; } = VctoonPermissions.Video.Default;
    protected override string? GetListPolicyName { get; set; } = VctoonPermissions.Video.Default;
    protected override string? CreatePolicyName { get; set; } = VctoonPermissions.Video.Create;
    protected override string? UpdatePolicyName { get; set; } = VctoonPermissions.Video.Update;
    protected override string? DeletePolicyName { get; set; } = VctoonPermissions.Video.Delete;

    // 原为布尔谓词，现改为属性选择器
    protected override LambdaExpression ProcessKeySelector =>
        (Expression<Func<IdentityUserReadingProcess, Guid?>>)(p => p.VideoId);

    [RemoteService(false)]
    public override Task<VideoDto> CreateAsync(VideoCreateUpdateDto input)
    {
        throw new UserFriendlyException("Not supported");
    }

#if !DEBUG
    [Authorize]
#endif
    [HttpGet]
    [HttpHead]
    public async Task<FileResult> GetVideoStreamAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new UserFriendlyException("Video id is empty");
        }

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Path, x.LibraryId });

        var video = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (video == null)
        {
            throw new UserFriendlyException("Video not found");
        }

#if !DEBUG
        await CheckCurrentUserLibraryPermissionAsync(video.LibraryId, x => x.CanView);
#endif

        if (!File.Exists(video.Path))
        {
            throw new UserFriendlyException("Video file not found");
        }

        var contentType = ConverterHelper.MapToRemoteContentType(video.Path);
        var result = new PhysicalFileResult(video.Path, contentType)
        {
            EnableRangeProcessing = true
        };

        return result;
    }

#if !DEBUG
    [Authorize]
#endif
    [HttpGet]
    [Route("{id}/subtitles")] // GET /api/app/video/{id}/subtitles
    public async Task<List<SubtitleDto>> GetSubtitlesAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new UserFriendlyException("Video id is empty");
        }

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.Id == id)
            .Select(x => new { x.Path, x.LibraryId });

        var video = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (video == null)
        {
            throw new UserFriendlyException("Video not found");
        }

#if !DEBUG
        await CheckCurrentUserLibraryPermissionAsync(video.LibraryId, x => x.CanView);
#endif
        if (string.IsNullOrWhiteSpace(video.Path) || !File.Exists(video.Path))
        {
            return new List<SubtitleDto>();
        }

        var dir = Path.GetDirectoryName(video.Path)!;
        var baseName = Path.GetFileNameWithoutExtension(video.Path);

        var candidates = Directory.EnumerateFiles(dir, baseName + "*.*", SearchOption.TopDirectoryOnly)
            .Where(p => IsSubtitleExtension(Path.GetExtension(p)))
            .ToList();

        var results = new List<SubtitleDto>(candidates.Count);
        foreach (var file in candidates)
        {
            var fileName = Path.GetFileName(file);
            var (lang, label) = ParseLanguageFromFileName(fileName);
            var ext = Path.GetExtension(file).Trim('.').ToLowerInvariant();
            results.Add(new SubtitleDto
            {
                FileName = fileName,
                Language = lang,
                Label = label ?? lang ?? ext,
                Format = ext,
                Url = $"/api/app/video/{id}/subtitles/{Uri.EscapeDataString(fileName)}"
            });
        }

        return results;
    }

#if !DEBUG
    [Authorize]
#endif
    [HttpGet]
    [Route("{id}/subtitles/{file}")] // GET /api/app/video/{id}/subtitles/{file}
    public async Task<IRemoteStreamContent> GetSubtitleAsync(Guid id, string file)
    {
        if (id == Guid.Empty)
        {
            throw new UserFriendlyException("Video id is empty");
        }

        if (string.IsNullOrWhiteSpace(file))
        {
            throw new UserFriendlyException("Subtitle file is empty");
        }

        if (!IsSafeFileName(file))
        {
            throw new UserFriendlyException("Invalid subtitle file name");
        }

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.Id == id)
            .Select(x => new { x.Path, x.LibraryId });

        var video = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (video == null)
        {
            throw new UserFriendlyException("Video not found");
        }

#if !DEBUG
        await CheckCurrentUserLibraryPermissionAsync(video.LibraryId, x => x.CanView);
#endif

        if (string.IsNullOrWhiteSpace(video.Path) || !File.Exists(video.Path))
        {
            throw new UserFriendlyException("Video file not found");
        }

        var dir = Path.GetDirectoryName(video.Path)!;
        var baseName = Path.GetFileNameWithoutExtension(video.Path);
        var fullPath = Path.Combine(dir, file);

        // 安全约束：必须在同目录且文件名以基名开头，且扩展名允许
        if (!fullPath.StartsWith(dir, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileNameWithoutExtension(fullPath).StartsWith(baseName, StringComparison.OrdinalIgnoreCase) ||
            !IsSubtitleExtension(Path.GetExtension(fullPath)))
        {
            throw new UserFriendlyException("Invalid subtitle request");
        }

        if (!File.Exists(fullPath))
        {
            throw new UserFriendlyException("Subtitle not found");
        }

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        // 如果是 vtt，直接返回；srt/ass 可按需转换成 vtt
        if (ext == ".vtt")
        {
            var stream = File.OpenRead(fullPath);
            return new RemoteStreamContent(stream, contentType: "text/vtt");
        }

        if (ext is ".srt" or ".ass")
        {
            var text = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
            var vtt = ext == ".srt" ? ConvertSrtToVtt(text) : ConvertAssToVtt(text);
            var bytes = Encoding.UTF8.GetBytes(vtt);
            var ms = new MemoryStream(bytes);
            ms.Position = 0;
            return new RemoteStreamContent(ms, contentType: "text/vtt");
        }

        throw new UserFriendlyException("Unsupported subtitle format");
    }

    private static bool IsSubtitleExtension(string ext)
        => ext.Equals(".srt", StringComparison.OrdinalIgnoreCase)
           || ext.Equals(".vtt", StringComparison.OrdinalIgnoreCase)
           || ext.Equals(".ass", StringComparison.OrdinalIgnoreCase);

    private static bool IsSafeFileName(string file)
    {
        if (file.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
        return !file.Contains('/') && !file.Contains('\\');
    }

    private static (string? lang, string? label) ParseLanguageFromFileName(string fileName)
    {
        // 支持 basename.zh.srt 或 basename.zh-CN.srt 或 basename.srt（无语言）
        var name = Path.GetFileNameWithoutExtension(fileName);
        var parts = name.Split('.');
        if (parts.Length >= 2)
        {
            var maybeLang = parts[^1];
            if (Regex.IsMatch(maybeLang, "^[A-Za-z]{2,}(-[A-Za-z0-9]{2,})?$"))
            {
                return (maybeLang.ToLowerInvariant(), maybeLang);
            }
        }

        return (null, null);
    }

    private static string ConvertSrtToVtt(string srt)
    {
        // 简单可靠的 SRT -> VTT 转换：
        // - 去掉纯数字序号行
        // - 将 00:00:01,000 -> 00:00:01.000
        // - 添加 WEBVTT 头
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();

        using var reader = new StringReader(srt);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                sb.AppendLine();
                continue;
            }

            // 跳过纯数字序号
            if (int.TryParse(line.Trim(), out _))
            {
                continue;
            }

            // 时间行替换逗号为点
            if (line.Contains("-->") && line.Contains(","))
            {
                line = line.Replace(',', '.');
            }

            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    private static string ConvertAssToVtt(string ass)
    {
        // 仅做非常基础的 ASS -> VTT 转换：提取 Dialogue 行的文本与时间
        // 更完整的转换应使用专业库，这里保持最小可用
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();

        // Dialogue: Marked=0,0:00:03.17,0:00:05.64,Default,,0,0,0,,Text
        var regex = new Regex(
            @"^Dialogue:[^,]*,(?<start>\d+:\d{2}:\d{2}([\.\,]\d{1,3})?),(?<end>\d+:\d{2}:\d{2}([\.\,]\d{1,3})?),[^,]*,[^,]*,[^,]*,[^,]*,[^,]*,(?<text>.*)$",
            RegexOptions.Compiled);

        using var reader = new StringReader(ass);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var m = regex.Match(line);
            if (!m.Success) continue;
            var start = m.Groups["start"].Value.Replace(',', '.');
            var end = m.Groups["end"].Value.Replace(',', '.');
            var text = m.Groups["text"].Value;
            text = StripAssTags(text);

            sb.AppendLine($"{start} --> {end}");
            sb.AppendLine(text);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string StripAssTags(string input)
    {
        // 去除 {\xxx} 样式标签与换行符号 \N
        var noTags = Regex.Replace(input, @"\{\\.*?\}", string.Empty);
        noTags = noTags.Replace("\\N", "\n");
        return noTags;
    }
}