using Microsoft.AspNetCore.Mvc;
using Vctoon.Mediums.Dtos;
using Volo.Abp;

namespace Vctoon.Mediums;

public partial class MediumAppService
{
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

    public virtual async Task<MediumDto> UpdateArtistsAsync(Guid id, List<Guid> artistIds)
    {
        await CheckUpdatePolicyAsync();

        artistIds ??= new List<Guid>();
        artistIds = artistIds.Distinct().Where(g => g != Guid.Empty).ToList();

        var entity = await GetEntityByIdAsync(id);

        var artistQuery = await artistRepository.GetQueryableAsync();
        var artists = await AsyncExecuter.ToListAsync(artistQuery.Where(a => artistIds.Contains(a.Id)));

        entity.Artists.RemoveAll(a => !artistIds.Contains(a.Id));
        foreach (var artist in artists)
        {
            if (entity.Artists.All(a => a.Id != artist.Id))
            {
                entity.Artists.Add(artist);
            }
        }

        await Repository.UpdateAsync(entity, true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public virtual async Task<MediumDto> UpdateTagsAsync(Guid id, List<Guid> tagIds)
    {
        await CheckUpdatePolicyAsync();

        tagIds ??= new List<Guid>();
        tagIds = tagIds.Distinct().Where(g => g != Guid.Empty).ToList();

        var entity = await GetEntityByIdAsync(id);

        var tagQuery = await tagRepository.GetQueryableAsync();
        var tags = await AsyncExecuter.ToListAsync(tagQuery.Where(t => tagIds.Contains(t.Id)));

        entity.Tags.RemoveAll(t => !tagIds.Contains(t.Id));
        foreach (var tag in tags)
        {
            if (entity.Tags.All(t => t.Id != tag.Id))
            {
                entity.Tags.Add(tag);
            }
        }

        await Repository.UpdateAsync(entity, true);
        return await MapToGetOutputDtoAsync(entity);
    }

    private enum RelationKind
    {
        Artist,
        Tag
    }

    private enum RelationOp
    {
        Add,
        Update,
        Delete
    }

    private async Task ModifyRelationsAsync(MediumMultiUpdateDto input, RelationKind kind, RelationOp op)
    {
        if (input == null)
        {
            throw new UserFriendlyException("Input is null");
        }

        if (input.MediumIds.IsNullOrEmpty())
        {
            throw new UserFriendlyException("Items is empty");
        }

        if (op != RelationOp.Delete && input.ResourceIds.IsNullOrEmpty())
        {
            throw new UserFriendlyException("Ids is empty");
        }

        var distinctMediumItems = input.MediumIds
            .Where(i => i != Guid.Empty)
            .ToList();

        if (!distinctMediumItems.Any())
        {
            return;
        }

        var targetIds = input.ResourceIds.Distinct().ToList();

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

        await HandleMediumSetAsync(distinctMediumItems, kind, op, allArtists, allTags);
    }

    private async Task HandleMediumSetAsync(
        List<Guid> mediumIds,
        RelationKind kind,
        RelationOp op,
        List<Artist> allArtists,
        List<Tag> allTags)
    {
        var list = await mediumRepository.GetListAsync(c => mediumIds.Contains(c.Id), includeDetails: true);
        foreach (var medium in list)
        {
            ApplyRelationChange(medium, kind, op, allArtists, allTags);
            await mediumRepository.UpdateAsync(medium, true);
        }
    }

    private void ApplyRelationChange(Medium medium, RelationKind kind, RelationOp op, List<Artist> allArtists,
        List<Tag> allTags)
    {
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
                    foreach (var remove in medium.Artists.Where(a => allArtists.All(x => x.Id != a.Id)).ToList())
                    {
                        medium.Artists.Remove(remove);
                    }
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

            return;
        }

        var currentTagIds = medium.Tags.Select(t => t.Id).ToHashSet();
        switch (op)
        {
            case RelationOp.Add:
                foreach (var t in allTags.Where(t => !currentTagIds.Contains(t.Id)))
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
