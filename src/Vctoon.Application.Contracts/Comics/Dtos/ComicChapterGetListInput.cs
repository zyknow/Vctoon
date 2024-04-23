using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicChapterGetListInput : PagedAndSortedResultRequestDto
{
    public string? Title { get; set; }

    public uint? PageCount { get; set; }

    public uint? Size { get; set; }

    public Guid? ComicId { get; set; }
}