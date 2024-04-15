using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicGetListInput : PagedAndSortedResultRequestDto
{
    public string? Title { get; set; }

    public string? CoverPath { get; set; }

    public Guid? LibraryId { get; set; }
}