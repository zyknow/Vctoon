using Vctoon.Mediums.Dtos;

namespace Vctoon.Mediums;

public interface IVideoAppService :
    IMediumBaseAppService<VideoDto, VideoGetListOutputDto, VideoGetListInput,
        VideoCreateUpdateDto, VideoCreateUpdateDto>
{
    Task<List<SubtitleDto>> GetSubtitlesAsync(Guid id);

    Task<IRemoteStreamContent> GetSubtitleAsync(Guid id, string file);
}