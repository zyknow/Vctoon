using Riok.Mapperly.Abstractions;
using Vctoon.Identities;
using Vctoon.Mediums;
using Vctoon.Mediums.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class ComicToComicDtoMapper : MapperBase<Comic, ComicDto>
{
    [MapProperty(nameof(Comic.Processes), nameof(ComicDto.ReadingProgress), Use = nameof(MapReadingProgress))]
    [MapProperty(nameof(Comic.Processes), nameof(ComicDto.ReadingLastTime), Use = nameof(MapReadingLastTime))]
    [MapperIgnoreSource(nameof(Comic.Tags))]
    [MapperIgnoreSource(nameof(Comic.Artists))]
    [MapperIgnoreSource(nameof(Comic.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(Comic.ComicImages))]
    public override partial ComicDto Map(Comic source);

    [MapperIgnoreSource(nameof(Comic.Tags))]
    [MapperIgnoreSource(nameof(Comic.Artists))]
    [MapperIgnoreSource(nameof(Comic.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(Comic.ComicImages))]
    public override partial void Map(Comic source, ComicDto destination);

    private static double? MapReadingProgress(ICollection<IdentityUserReadingProcess> processes)
    {
        return processes.Select(p => (double?)p.Progress).FirstOrDefault();
    }

    private static DateTime? MapReadingLastTime(ICollection<IdentityUserReadingProcess> processes)
    {
        return processes.Select(p => p.LastReadTime).FirstOrDefault();
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class ComicToComicGetListOutputDtoMapper : MapperBase<Comic, ComicGetListOutputDto>
{
    [MapProperty(nameof(Comic.Processes), nameof(ComicGetListOutputDto.ReadingProgress),
        Use = nameof(MapReadingProgress))]
    [MapProperty(nameof(Comic.Processes), nameof(ComicGetListOutputDto.ReadingLastTime),
        Use = nameof(MapReadingLastTime))]
    [MapperIgnoreSource(nameof(Comic.Tags))]
    [MapperIgnoreSource(nameof(Comic.Artists))]
    [MapperIgnoreSource(nameof(Comic.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(Comic.ComicImages))]
    public override partial ComicGetListOutputDto Map(Comic source);

    [MapperIgnoreSource(nameof(Comic.Tags))]
    [MapperIgnoreSource(nameof(Comic.Artists))]
    [MapperIgnoreSource(nameof(Comic.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(Comic.ComicImages))]
    public override partial void Map(Comic source, ComicGetListOutputDto destination);

    private static double? MapReadingProgress(ICollection<IdentityUserReadingProcess> processes)
    {
        return processes.Select(p => (double?)p.Progress).FirstOrDefault();
    }

    private static DateTime? MapReadingLastTime(ICollection<IdentityUserReadingProcess> processes)
    {
        return processes.Select(p => p.LastReadTime).FirstOrDefault();
    }
}