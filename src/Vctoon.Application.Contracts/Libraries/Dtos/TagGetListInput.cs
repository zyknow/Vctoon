using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagGetListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }
}