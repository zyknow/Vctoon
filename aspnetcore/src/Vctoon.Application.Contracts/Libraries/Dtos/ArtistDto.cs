namespace Vctoon.Libraries.Dtos;

[Serializable]
public class ArtistDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }

    public string Description { get; set; }
}