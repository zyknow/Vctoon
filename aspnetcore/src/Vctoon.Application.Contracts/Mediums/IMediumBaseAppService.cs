using Volo.Abp.Content;

namespace Vctoon.Mediums;

public interface IMediumBaseAppService<TGetOutputDto, TGetListOutputDto, in TGetListInput, in TCreateInput,
    in TUpdateInput> : ICrudAppService<
    TGetOutputDto,
    TGetListOutputDto,
    Guid,
    TGetListInput,
    TCreateInput,
    TUpdateInput>
{
    // 新增：更新封面
    Task<TGetOutputDto> UpdateCoverAsync(Guid id, RemoteStreamContent cover);

    // 新增：更新作者（传入作者ID集合，整体替换）
    Task<TGetOutputDto> UpdateArtistsAsync(Guid id, List<Guid> artistIds);

    // 新增：更新标签（传入标签ID集合，整体替换）
    Task<TGetOutputDto> UpdateTagsAsync(Guid id, List<Guid> tagIds);
}