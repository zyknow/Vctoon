using Riok.Mapperly.Abstractions;
using Vctoon.Libraries.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class ComicImageToComicImageDtoMapper : MapperBase<ComicImage, ComicImageDto>
{
    public override partial ComicImageDto Map(ComicImage source);
    public override partial void Map(ComicImage source, ComicImageDto destination);
}