using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicDto : EntityDto<Guid>
{
    public string Title { get; set; }

    public string CoverPath { get; set; }

    public Guid LibraryId { get; set; }

    public double CompletionRate { get; set; }

    // public List<Tag> Tags { get; set; }
    //
    // public List<ComicChapter> Chapters { get; set; }
}