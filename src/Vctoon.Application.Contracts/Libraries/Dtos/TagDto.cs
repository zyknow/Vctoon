using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }
}