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
    protected override string GetPolicyName { get; set; } = VctoonPermissions.Video.Default;
    protected override string GetListPolicyName { get; set; } = VctoonPermissions.Video.Default;
    protected override string CreatePolicyName { get; set; } = VctoonPermissions.Video.Create;
    protected override string UpdatePolicyName { get; set; } = VctoonPermissions.Video.Update;
    protected override string DeletePolicyName { get; set; } = VctoonPermissions.Video.Delete;


    [RemoteService(false)]
    public override Task<VideoDto> CreateAsync(VideoCreateUpdateDto input)
    {
        throw new UserFriendlyException("Not supported");
    }

    protected override async Task<IQueryable<Video>> CreateFilteredQueryAsync(VideoGetListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        if (CurrentUser.Id != null)
        {
            query = query.WhereIf(input.HasReadingProgress != null,
                x => x.Processes.Any(p => p.VideoId != null && p.UserId == CurrentUser.Id && p.Progress > 0) ==
                     input.HasReadingProgress);
        }

        return query;
    }
}