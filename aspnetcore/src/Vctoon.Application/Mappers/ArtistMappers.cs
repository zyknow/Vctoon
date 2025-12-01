using Riok.Mapperly.Abstractions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class ArtistToArtistDtoMapper : MapperBase<Artist, ArtistDto>
{
    public override partial ArtistDto Map(Artist source);
    public override partial void Map(Artist source, ArtistDto destination);
}