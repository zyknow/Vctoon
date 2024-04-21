using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryGetListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }
}