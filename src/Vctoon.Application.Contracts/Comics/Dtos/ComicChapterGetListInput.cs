using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicChapterGetListInput : PagedAndSortedResultRequestDto
{
    public string? CoverPath { get; set; }

    public string? Title { get; set; }

    public Guid? ComicId { get; set; }

    public uint? PageCount { get; set; }

    public uint? Size { get; set; }

    // public LibraryPathDto? LibraryPath { get; set; }
}