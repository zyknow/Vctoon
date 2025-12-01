using Riok.Mapperly.Abstractions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class LibraryPermissionToLibraryPermissionDtoMapper : MapperBase<LibraryPermission, LibraryPermissionDto>
{
    public override partial LibraryPermissionDto Map(LibraryPermission source);
    public override partial void Map(LibraryPermission source, LibraryPermissionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class
    LibraryPermissionCreateUpdateDtoToLibraryPermissionMapper : MapperBase<LibraryPermissionCreateUpdateDto,
    LibraryPermission>
{
    public override partial LibraryPermission Map(LibraryPermissionCreateUpdateDto source);
    public override partial void Map(LibraryPermissionCreateUpdateDto source, LibraryPermission destination);
}