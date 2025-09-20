namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagGetListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }
}