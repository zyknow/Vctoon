using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryCollectionGetListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }

    public string? CoverPath { get; set; }
}