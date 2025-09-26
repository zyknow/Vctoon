namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}