using System;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicDto : EntityDto<Guid>
{
    public string Title { get; set; }

    public string CoverPath { get; set; }

    public Guid LibraryId { get; set; }

    public List<ComicChapterDto> Chapters { get; set; }
}