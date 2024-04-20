using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagGroupGetListInput : PagedAndSortedResultRequestDto
{
    public string Name { get; set; }

}