using Vctoon.Mediums.Dtos;
using Volo.Abp.Content;

namespace Vctoon.Mediums;

public interface IVideoAppService :
    IMediumBaseAppService<VideoDto, VideoGetListOutputDto, VideoGetListInput,
        VideoCreateUpdateDto, VideoCreateUpdateDto>
{
    Task<List<SubtitleDto>> GetSubtitlesAsync(Guid id);

    Task<IRemoteStreamContent> GetSubtitleAsync(Guid id, string file);
}