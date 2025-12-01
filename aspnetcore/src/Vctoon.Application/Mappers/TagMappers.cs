using Riok.Mapperly.Abstractions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class TagToTagDtoMapper : MapperBase<Tag, TagDto>
{
    [MapperIgnoreSource(nameof(Tag.LastModificationTime))]
    [MapperIgnoreSource(nameof(Tag.LastModifierId))]
    [MapperIgnoreSource(nameof(Tag.CreationTime))]
    [MapperIgnoreSource(nameof(Tag.CreatorId))]
    public override partial TagDto Map(Tag source);

    [MapperIgnoreSource(nameof(Tag.LastModificationTime))]
    [MapperIgnoreSource(nameof(Tag.LastModifierId))]
    [MapperIgnoreSource(nameof(Tag.CreationTime))]
    [MapperIgnoreSource(nameof(Tag.CreatorId))]
    public override partial void Map(Tag source, TagDto destination);
}