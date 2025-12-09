namespace Vctoon.Mediums.Dtos;

public class MediumMultiUpdateDto
{

    public List<Guid> MediumIds { get; set; } = [];

    /// <summary>
    /// Tag/Artist/... Ids
    /// </summary>
    public List<Guid> ResourceIds { get; set; } = [];
}