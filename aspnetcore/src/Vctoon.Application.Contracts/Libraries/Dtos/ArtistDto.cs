namespace Vctoon.Libraries.Dtos;

[Serializable]
public class ArtistDto : EntityDto<Guid>
{
    public string Name { get; set; }
}