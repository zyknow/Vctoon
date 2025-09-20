namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public long Slug { get; set; }

    public long? ResourceCount { get; set; }
}