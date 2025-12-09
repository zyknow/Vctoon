using Riok.Mapperly.Abstractions;
using Vctoon.Identities;
using Vctoon.Mediums;
using Vctoon.Mediums.Dtos;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class MediumToMediumDtoMapper : MapperBase<Medium, MediumDto>
{
    [MapProperty(nameof(Medium.Processes), nameof(MediumDto.ReadingProgress), Use = nameof(MapReadingProgress))]
    [MapProperty(nameof(Medium.Processes), nameof(MediumDto.ReadingLastTime), Use = nameof(MapReadingLastTime))]
    public override partial MediumDto Map(Medium source);

    public override partial void Map(Medium source, MediumDto destination);


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
public partial class MediumToMediumGetListOutputDtoMapper : MapperBase<Medium, MediumGetListOutputDto>
{
    [MapProperty(nameof(Medium.Processes), nameof(MediumDto.ReadingProgress), Use = nameof(MapReadingProgress))]
    [MapProperty(nameof(Medium.Processes), nameof(MediumDto.ReadingLastTime), Use = nameof(MapReadingLastTime))]
    public override partial MediumGetListOutputDto Map(Medium source);

    public override partial void Map(Medium source, MediumGetListOutputDto destination);


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
public partial class CreateUpdateMediumDtoToMediumMapper : MapperBase<CreateUpdateMediumDto, Medium>
{
    public override Medium Map(CreateUpdateMediumDto source)
    {
        var medium = new Medium(
            Guid.NewGuid(),
            source.MediumType,
            source.Title,
            source.Cover,
            source.LibraryId,
            source.LibraryPathId,
            source.Description
        )
        {
            MediumType = source.MediumType,
            VideoDetail = source.VideoDetail
        };

        return medium;
    }


    [MapperIgnoreTarget(nameof(Medium.Id))]
    [MapperIgnoreTarget(nameof(Medium.ExtraProperties))]
    [MapperIgnoreTarget(nameof(Medium.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(Medium.CreationTime))]
    [MapperIgnoreTarget(nameof(Medium.CreatorId))]
    [MapperIgnoreTarget(nameof(Medium.LastModificationTime))]
    [MapperIgnoreTarget(nameof(Medium.LastModifierId))]
    [MapperIgnoreTarget(nameof(Medium.Tags))]
    [MapperIgnoreTarget(nameof(Medium.Artists))]
    [MapperIgnoreTarget(nameof(Medium.Processes))]
    [MapperIgnoreTarget(nameof(Medium.ReadCount))]
    public override partial void Map(CreateUpdateMediumDto source, Medium destination);
}