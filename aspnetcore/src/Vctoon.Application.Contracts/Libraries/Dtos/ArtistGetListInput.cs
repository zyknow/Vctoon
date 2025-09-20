namespace Vctoon.Libraries.Dtos;

[Serializable]
public class ArtistGetListInput : PagedAndSortedResultRequestDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}