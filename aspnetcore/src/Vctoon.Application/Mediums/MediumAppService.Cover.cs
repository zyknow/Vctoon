using Microsoft.AspNetCore.Mvc;
using Vctoon.Helper;
using Vctoon.Mediums.Dtos;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Content;

namespace Vctoon.Mediums;

public partial class MediumAppService
{
#if !DEBUG
    [Authorize]
#endif
    public async Task<RemoteStreamContent> GetCoverAsync(string cover)
    {
        var stream = await coverContainer.GetAsync(cover);
        return new RemoteStreamContent(stream, contentType: ConverterHelper.MapToRemoteContentType(cover));
    }

    public virtual async Task<MediumDto> UpdateCoverAsync(Guid id, RemoteStreamContent cover)
    {
        await CheckUpdatePolicyAsync();

        if (cover == null)
        {
            throw new UserFriendlyException("cover is null");
        }

        var entity = await GetEntityByIdAsync(id);

        await using var stream = cover.GetStream();
        if (stream == null || stream.Length == 0)
        {
            throw new UserFriendlyException("cover stream is empty");
        }

        var newCover = await coverSaver.SaveAsync(stream);
        entity.Cover = newCover;

        await Repository.UpdateAsync(entity, true);
        return await MapToGetOutputDtoAsync(entity);
    }
}
