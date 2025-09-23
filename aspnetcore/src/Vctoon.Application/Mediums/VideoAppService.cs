using System.Linq.Expressions;
using Vctoon.Identities;
using Vctoon.Mediums.Base;
using Vctoon.Mediums.Dtos;
using Vctoon.Permissions;
using Volo.Abp;

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

    protected override Expression<Func<IdentityUserReadingProcess, bool>> ProcessPredicate => p => p.VideoId != null;

    [RemoteService(false)]
    public override Task<VideoDto> CreateAsync(VideoCreateUpdateDto input)
    {
        throw new UserFriendlyException("Not supported");
    }
}