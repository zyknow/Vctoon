using Vctoon.Identities;
using Vctoon.Mediums.Dtos;
using Volo.Abp;
using Volo.Abp.Authorization;

namespace Vctoon.Mediums;

public partial class MediumAppService
{
    [Authorize]
    public async Task UpdateReadingProcessAsync(List<ReadingProcessUpdateDto> items)
    {
        if (items.IsNullOrEmpty())
        {
            throw new UserFriendlyException(L["InputListIsEmpty"]);
        }

        if (items.Any(x => x.MediumId == Guid.Empty))
        {
            throw new UserFriendlyException(L["OneOrMoreItemsHaveEmptyMediumId"]);
        }

        if (items.Any(x => x.Progress < 0 || x.Progress > 1))
        {
            throw new UserFriendlyException(L["InvalidProgressValue"]);
        }

        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException(L["UserNotAuthenticated"]);
        }

        var userId = CurrentUser.Id;
        var query = await readingProcessRepository.GetQueryableAsync();
        var mediumIds = items.Select(x => x.MediumId).Distinct().ToList();

        var dbProcessList = await AsyncExecuter.ToListAsync(
            query.Where(x => x.UserId == userId && mediumIds.Contains(x.MediumId))
        );

        var addProcessDtoList = items.Where(x => dbProcessList.All(y => y.MediumId != x.MediumId)).ToList();
        var addProcess = addProcessDtoList.Select(x => new IdentityUserReadingProcess(
            GuidGenerator.Create(),
            userId!.Value,
            x.MediumId,
            x.Progress
        )).ToList();

        if (addProcess.Any())
        {
            await readingProcessRepository.InsertManyAsync(addProcess, true);
        }

        var addedMediumIds = addProcessDtoList.Select(x => x.MediumId).ToHashSet();
        var updateProcessDtoList = items.Where(x => !addedMediumIds.Contains(x.MediumId)).ToList();
        var updateProcess = dbProcessList.Where(x => updateProcessDtoList.Any(y => y.MediumId == x.MediumId)).ToList();

        foreach (var process in updateProcess)
        {
            var dto = updateProcessDtoList.First(x => x.MediumId == process.MediumId);
            process.Progress = dto.Progress;
            process.LastReadTime = dto.ReadingLastTime;
        }

        if (updateProcess.Any())
        {
            await readingProcessRepository.UpdateManyAsync(updateProcess, true);
        }

        // 更新媒介的阅读次数（只要有一次进度写入且非 0，就 +1）
        var mediumIdsToUpdate = items.Where(x => x.Progress != 0)
            .Select(x => x.MediumId)
            .Distinct()
            .ToList();

        await mediumRepository.AdditionReadCountAsync(mediumIdsToUpdate);
    }
}
