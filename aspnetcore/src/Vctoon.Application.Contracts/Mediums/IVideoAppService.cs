using Vctoon.Mediums.Dtos;

namespace Vctoon.Mediums;

public interface IVideoAppService :
    IMediumBaseAppService<VideoDto, VideoGetListOutputDto, VideoGetListInput,
        VideoCreateUpdateDto, VideoCreateUpdateDto>
{
}