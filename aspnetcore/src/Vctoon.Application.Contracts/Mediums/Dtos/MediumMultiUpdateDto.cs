namespace Vctoon.Mediums.Dtos;

public class MediumMultiUpdateDto
{
    public List<MediumMultiUpdateItemDto> Items { get; set; } = [];

    /// <summary>
    /// Tag/Artist/... Ids
    /// </summary>
    public List<Guid> Ids { get; set; } = [];
}