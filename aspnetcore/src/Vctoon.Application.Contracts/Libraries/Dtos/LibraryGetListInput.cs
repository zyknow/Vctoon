using Vctoon.Mediums;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryGetListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }

    public MediumType? MediumType { get; set; }
}