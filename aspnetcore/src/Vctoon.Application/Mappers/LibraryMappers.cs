using Riok.Mapperly.Abstractions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class LibraryToLibraryDtoMapper : MapperBase<Library, LibraryDto>
{
    [MapperIgnoreTarget(nameof(LibraryDto.Paths))]
    [MapperIgnoreSource(nameof(Library.Paths))]
    public override partial LibraryDto Map(Library source);

    [MapperIgnoreSource(nameof(Library.Paths))]
    public override partial void Map(Library source, LibraryDto destination);
}