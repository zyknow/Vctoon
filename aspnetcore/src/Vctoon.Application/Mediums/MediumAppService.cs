using Vctoon.BlobContainers;
using Vctoon.Identities;
using Vctoon.Mediums.Dtos;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.BlobStoring;
// for UserFriendlyException

// for AbpAuthorizationException

namespace Vctoon.Mediums;

public class MediumAppService(
    IBlobContainer<CoverContainer> coverContainer,
    IIdentityUserReadingProcessRepository readingProcessRepository
) : VctoonAppService
{
#if !DEBUG
    [Authorize]
#endif
    public async Task<Stream> GetCoverAsync(string cover)
    {
        var stream = await coverContainer.GetAsync(cover);
        return stream;
    }

    [Authorize]
    public async Task UpdateReadingProcessAsync(ReadingProcessUpdateDto input)
    {
        if (input.MediumId == Guid.Empty)
        {
            throw new UserFriendlyException("MediumId is empty");
        }

        if (input.Progress < 0 || input.Progress > 1)
        {
            throw new UserFriendlyException("Progress must be between 0 and 1");
        }

        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not authenticated");
        }

        var currentUserId = CurrentUser.Id.Value;

        // 写入/更新阅读进度（以 UserId + MediumId 作为复合键 Upsert）
        var query = await readingProcessRepository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.UserId == currentUserId && x.MediumId == input.MediumId)
        );

        if (entity == null)
        {
            entity = new IdentityUserReadingProcess(
                currentUserId,
                input.MediumId
            );

            await readingProcessRepository.InsertAsync(entity, true);
        }
        else
        {
            entity.Progress = input.Progress;
            entity.LastReadTime = input.LastReadTime;
            await readingProcessRepository.UpdateAsync(entity, true);
        }
    }
}