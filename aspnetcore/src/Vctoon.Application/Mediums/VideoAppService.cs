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

    public override Task<VideoDto> CreateAsync(VideoCreateUpdateDto input)
    {
        throw new UserFriendlyException("Not supported");
    }

    protected override async Task<IQueryable<Video>> CreateFilteredQueryAsync(VideoGetListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        // TODO: AbpHelper generated
        return query
                .WhereIf(input.Framerate != null, x => x.Framerate == input.Framerate)
                .WhereIf(!input.Codec.IsNullOrWhiteSpace(), x => x.Codec.Contains(input.Codec))
                .WhereIf(input.Width != null, x => x.Width == input.Width)
                .WhereIf(input.Height != null, x => x.Height == input.Height)
                .WhereIf(input.Bitrate != null, x => x.Bitrate == input.Bitrate)
                .WhereIf(!input.Ratio.IsNullOrWhiteSpace(), x => x.Ratio.Contains(input.Ratio))
                .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title))
                .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
            ;
    }
}