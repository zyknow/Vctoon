namespace Vctoon.Mediums.Dtos;

public class MediumMultiUpdateDto
{
    public List<MediumMultiUpdateItemDto> Items { get; set; } = [];

    public List<Guid> Ids { get; set; } = [];
}