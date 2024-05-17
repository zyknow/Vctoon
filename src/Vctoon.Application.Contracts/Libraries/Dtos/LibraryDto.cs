using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public List<string> Paths { get; set; } = [];
}