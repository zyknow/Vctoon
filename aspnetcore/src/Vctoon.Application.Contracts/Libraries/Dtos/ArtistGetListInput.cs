namespace Vctoon.Libraries.Dtos;

[Serializable]
public class ArtistGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}