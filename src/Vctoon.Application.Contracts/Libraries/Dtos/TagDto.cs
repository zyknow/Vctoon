using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public int? ResourceCount { get; set; } = null;
}