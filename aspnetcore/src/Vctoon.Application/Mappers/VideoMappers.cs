using Riok.Mapperly.Abstractions;
using Vctoon.Identities;
using Vctoon.Mediums;
using Vctoon.Mediums.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class VideoToVideoDtoMapper : MapperBase<Video, VideoDto>
{
    [MapProperty(nameof(Video.Processes), nameof(VideoDto.ReadingProgress), Use = nameof(MapReadingProgress))]
    [MapProperty(nameof(Video.Processes), nameof(VideoDto.ReadingLastTime), Use = nameof(MapReadingLastTime))]
    [MapperIgnoreSource(nameof(Video.Tags))]
    [MapperIgnoreSource(nameof(Video.Artists))]
    [MapperIgnoreSource(nameof(Video.LibraryPathId))]
    [MapperIgnoreSource(nameof(Video.ExtraProperties))]
    [MapperIgnoreSource(nameof(Video.ConcurrencyStamp))]
    public override partial VideoDto Map(Video source);

    [MapperIgnoreSource(nameof(Video.Tags))]
    [MapperIgnoreSource(nameof(Video.Artists))]
    [MapperIgnoreSource(nameof(Video.LibraryPathId))]
    [MapperIgnoreSource(nameof(Video.ExtraProperties))]
    [MapperIgnoreSource(nameof(Video.ConcurrencyStamp))]
    public override partial void Map(Video source, VideoDto destination);

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
public partial class VideoToVideoGetListOutputDtoMapper : MapperBase<Video, VideoGetListOutputDto>
{
    [MapProperty(nameof(Video.Processes), nameof(VideoGetListOutputDto.ReadingProgress),
        Use = nameof(MapReadingProgress))]
    [MapProperty(nameof(Video.Processes), nameof(VideoGetListOutputDto.ReadingLastTime),
        Use = nameof(MapReadingLastTime))]
    [MapperIgnoreSource(nameof(Video.Tags))]
    [MapperIgnoreSource(nameof(Video.Artists))]
    [MapperIgnoreSource(nameof(Video.LibraryPathId))]
    [MapperIgnoreSource(nameof(Video.ExtraProperties))]
    [MapperIgnoreSource(nameof(Video.ConcurrencyStamp))]
    public override partial VideoGetListOutputDto Map(Video source);

    [MapperIgnoreSource(nameof(Video.Tags))]
    [MapperIgnoreSource(nameof(Video.Artists))]
    [MapperIgnoreSource(nameof(Video.LibraryPathId))]
    [MapperIgnoreSource(nameof(Video.ExtraProperties))]
    [MapperIgnoreSource(nameof(Video.ConcurrencyStamp))]
    public override partial void Map(Video source, VideoGetListOutputDto destination);

    private static double? MapReadingProgress(ICollection<IdentityUserReadingProcess> processes)
    {
        return processes.Select(p => (double?)p.Progress).FirstOrDefault();
    }

    private static DateTime? MapReadingLastTime(ICollection<IdentityUserReadingProcess> processes)
    {
        return processes.Select(p => p.LastReadTime).FirstOrDefault();
    }
}