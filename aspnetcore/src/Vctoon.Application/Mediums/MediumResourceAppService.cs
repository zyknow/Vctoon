using Microsoft.AspNetCore.Mvc;
using Vctoon.BlobContainers;
using Vctoon.Helper;
using Vctoon.Identities;
using Vctoon.Mediums.Dtos;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Vctoon.Mediums.Base; // 添加 MediumBase 引用

namespace Vctoon.Mediums;

public class MediumResourceAppService(
    IBlobContainer<CoverContainer> coverContainer,
    IIdentityUserReadingProcessRepository readingProcessRepository,
    IComicRepository comicRepository,
    IVideoRepository videoRepository,
    IRepository<Artist, Guid> artistRepository,
    ITagRepository tagRepository
) : VctoonAppService, IMediumResourceAppService
{
#if !DEBUG
    [Authorize]
#endif
    public async Task<RemoteStreamContent> GetCoverAsync(string cover)
    {
        var stream = await coverContainer.GetAsync(cover);
        // TODO: 根据实际图片类型返回正确的 ContentType
        return new RemoteStreamContent(stream, contentType: ConverterHelper.MapToRemoteContentType(cover));
    }

    [Authorize]
    public async Task UpdateReadingProcessAsync(List<ReadingProcessUpdateDto> items)
    {
        if (items.IsNullOrEmpty())
        {
            throw new UserFriendlyException("Input list is empty");
        }

        if (items.Any(x => x.MediumId == Guid.Empty))
        {
            throw new UserFriendlyException("One or more items have empty MediumId");
        }

        if (items.Any(x => x.Progress < 0 || x.Progress > 1))
        {
            throw new UserFriendlyException("One or more items have invalid Progress (must be between 0 and 1)");
        }

        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not authenticated");
        }

        // 写入/更新阅读进度（以 UserId + MediumId 作为复合键 Upsert）
        var query = await readingProcessRepository.GetQueryableAsync();

        var mediumIds = items.Select(x => x.MediumId).ToList();

        var dbProcessList = query.Where(x =>
                x.UserId == CurrentUser.Id &&
                (mediumIds.Contains(x.ComicId!.Value) || mediumIds.Contains(x.VideoId!.Value)))
            .ToList();

        var addProcessDtoList = items.Where(x => dbProcessList.All(y =>
            (x.MediumType == MediumType.Comic && y.ComicId != x.MediumId) ||
            (x.MediumType == MediumType.Video && y.VideoId != x.MediumId)
        )).ToList();

        var addProcess = addProcessDtoList.Select(x => new IdentityUserReadingProcess(
            GuidGenerator.Create(),
            CurrentUser.Id.Value,
            x.MediumId,
            x.MediumType,
            x.Progress
        )).ToList();

        if (addProcess.Any())
        {
            await readingProcessRepository.InsertManyAsync(addProcess, true);
        }

        var updateProcessDtoList = items.Except(addProcessDtoList).ToList();
        var updateProcess = dbProcessList.Where(x => updateProcessDtoList.Any(y =>
            (y.MediumType == MediumType.Comic && x.ComicId == y.MediumId) ||
            (y.MediumType == MediumType.Video && x.VideoId == y.MediumId)
        )).ToList();

        foreach (var process in updateProcess)
        {
            var dto = updateProcessDtoList.First(x =>
                (x.MediumType == MediumType.Comic && process.ComicId == x.MediumId) ||
                (x.MediumType == MediumType.Video && process.VideoId == x.MediumId)
            );
            process.Progress = dto.Progress;
            process.LastReadTime = dto.ReadingLastTime;
        }

        if (updateProcess.Any())
        {
            await readingProcessRepository.UpdateManyAsync(updateProcess, true);
        }

        // 更新媒介的阅读次数
        foreach (var readingProcessUpdateDtose in items.Where(x => x.Progress != 0).GroupBy(x => x.MediumType))
        {
            if (readingProcessUpdateDtose.Key == MediumType.Comic)
            {
                var comicIds = readingProcessUpdateDtose.Select(x => x.MediumId).ToList();
                await comicRepository.AdditionReadCountAsync(comicIds);
            }
            else if (readingProcessUpdateDtose.Key == MediumType.Video)
            {
                var videoIds = readingProcessUpdateDtose.Select(x => x.MediumId).ToList();
                await videoRepository.AdditionReadCountAsync(videoIds);
            }
        }
    }

    [HttpPut]
    public virtual async Task AddArtistListAsync(MediumMultiUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        await ModifyRelationsAsync(input, RelationKind.Artist, RelationOp.Add);
    }

    [HttpPost]
    public virtual async Task UpdateArtistListAsync(MediumMultiUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        await ModifyRelationsAsync(input, RelationKind.Artist, RelationOp.Update);
    }

    [HttpPut]
    public virtual async Task DeleteArtistListAsync(MediumMultiUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        await ModifyRelationsAsync(input, RelationKind.Artist, RelationOp.Delete);
    }

    [HttpPost]
    public virtual async Task UpdateTagListAsync(MediumMultiUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        await ModifyRelationsAsync(input, RelationKind.Tag, RelationOp.Update);
    }


    [HttpPut]
    public virtual async Task AddTagListAsync(MediumMultiUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        await ModifyRelationsAsync(input, RelationKind.Tag, RelationOp.Add);
    }

    [HttpPut]
    public virtual async Task DeleteTagListAsync(MediumMultiUpdateDto input)
    {
        await CheckUpdatePolicyAsync();
        await ModifyRelationsAsync(input, RelationKind.Tag, RelationOp.Delete);
    }


    protected virtual async Task CheckUpdatePolicyAsync()
    {
        if (!CurrentUser.Roles.Contains(VctoonSharedConsts.AdminUserRoleName.ToUpper()))
        {
            throw new AbpAuthorizationException("You don't have permission to update this resource");
        }
    }

    private enum RelationKind { Artist, Tag }
    private enum RelationOp { Add, Update, Delete }

    private async Task ModifyRelationsAsync(MediumMultiUpdateDto input, RelationKind kind, RelationOp op)
    {
        // 基础校验
        if (input == null)
        {
            throw new UserFriendlyException("Input is null");
        }
        if (input.Items.IsNullOrEmpty())
        {
            throw new UserFriendlyException("Items is empty");
        }
        if (op != RelationOp.Delete && input.Ids.IsNullOrEmpty())
        {
            throw new UserFriendlyException("Ids is empty");
        }

        var distinctMediumItems = input.Items
            .Where(i => i.Id != Guid.Empty)
            .DistinctBy(i => (i.MediumType, i.Id))
            .ToList();

        if (!distinctMediumItems.Any())
        {
            return; // 无可处理
        }

        var targetIds = input.Ids.Distinct().ToList();

        // 预取关系实体（Artist / Tag）
        List<Artist> allArtists = [];
        List<Tag> allTags = [];
        if (kind == RelationKind.Artist && targetIds.Any())
        {
            allArtists = await artistRepository.GetListAsync(a => targetIds.Contains(a.Id));
        }
        else if (kind == RelationKind.Tag && targetIds.Any())
        {
            allTags = await tagRepository.GetListAsync(t => targetIds.Contains(t.Id));
        }

        // 分类型处理
        var comicIds = distinctMediumItems.Where(x => x.MediumType == MediumType.Comic).Select(x => x.Id).ToList();
        var videoIds = distinctMediumItems.Where(x => x.MediumType == MediumType.Video).Select(x => x.Id).ToList();

        if (comicIds.Any())
        {
            await HandleMediumSetAsync(comicIds, kind, op, true, allArtists, allTags);
        }
        if (videoIds.Any())
        {
            await HandleMediumSetAsync(videoIds, kind, op, false, allArtists, allTags);
        }
    }

    private async Task HandleMediumSetAsync(
        List<Guid> mediumIds,
        RelationKind kind,
        RelationOp op,
        bool isComic,
        List<Artist> allArtists,
        List<Tag> allTags)
    {
        // 查询并包含需要的导航属性
        if (isComic)
        {
            var list = await comicRepository.GetListAsync(c => mediumIds.Contains(c.Id), includeDetails: true);
            foreach (var medium in list)
            {
                ApplyRelationChange(medium, kind, op, allArtists, allTags);
                await comicRepository.UpdateAsync(medium, true);
            }
        }
        else
        {
            var list = await videoRepository.GetListAsync(c => mediumIds.Contains(c.Id), includeDetails: true);
            foreach (var medium in list)
            {
                ApplyRelationChange(medium, kind, op, allArtists, allTags);
                await videoRepository.UpdateAsync(medium, true);
            }
        }
    }

    private void ApplyRelationChange(MediumBase medium, RelationKind kind, RelationOp op, List<Artist> allArtists, List<Tag> allTags)
    {
        // 根据操作对导航集合进行增删改
        if (kind == RelationKind.Artist)
        {
            var currentIds = medium.Artists.Select(a => a.Id).ToHashSet();
            switch (op)
            {
                case RelationOp.Add:
                    foreach (var a in allArtists.Where(a => !currentIds.Contains(a.Id)))
                    {
                        medium.Artists.Add(a);
                    }
                    break;
                case RelationOp.Update:
                    // 删除不存在的
                    foreach (var remove in medium.Artists.Where(a => allArtists.All(x => x.Id != a.Id)).ToList())
                    {
                        medium.Artists.Remove(remove);
                    }
                    // 添加缺少的
                    foreach (var add in allArtists.Where(a => medium.Artists.All(x => x.Id != a.Id)))
                    {
                        medium.Artists.Add(add);
                    }
                    break;
                case RelationOp.Delete:
                    foreach (var remove in medium.Artists.Where(a => allArtists.Any(x => x.Id == a.Id)).ToList())
                    {
                        medium.Artists.Remove(remove);
                    }
                    break;
            }
        }
        else // Tag
        {
            var currentIds = medium.Tags.Select(t => t.Id).ToHashSet();
            switch (op)
            {
                case RelationOp.Add:
                    foreach (var t in allTags.Where(t => !currentIds.Contains(t.Id)))
                    {
                        medium.Tags.Add(t);
                    }
                    break;
                case RelationOp.Update:
                    foreach (var remove in medium.Tags.Where(t => allTags.All(x => x.Id != t.Id)).ToList())
                    {
                        medium.Tags.Remove(remove);
                    }
                    foreach (var add in allTags.Where(t => medium.Tags.All(x => x.Id != t.Id)))
                    {
                        medium.Tags.Add(add);
                    }
                    break;
                case RelationOp.Delete:
                    foreach (var remove in medium.Tags.Where(t => allTags.Any(x => x.Id == t.Id)).ToList())
                    {
                        medium.Tags.Remove(remove);
                    }
                    break;
            }
        }
    }
}