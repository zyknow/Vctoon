using Vctoon.Mediums;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }

    public MediumType MediumType { get; set; }
    public List<string> Paths { get; set; } = [];
}