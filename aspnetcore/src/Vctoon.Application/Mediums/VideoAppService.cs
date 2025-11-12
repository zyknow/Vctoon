using System.Linq.Expressions;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Vctoon.Helper;
using Vctoon.Identities;
using Vctoon.Mediums.Base;
using Vctoon.Mediums.Dtos;
using Vctoon.Permissions;
using Volo.Abp;
using Volo.Abp.Authorization;

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
}
