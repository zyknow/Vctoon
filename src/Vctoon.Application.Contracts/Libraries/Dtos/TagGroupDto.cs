using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagGroupDto : EntityDto<Guid>
{
    public string Name { get; set; }

    // public List<Tag> Tags { get; set; }
}