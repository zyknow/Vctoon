using Riok.Mapperly.Abstractions;
using Vctoon.Identities.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;

namespace Vctoon.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
[MapExtraProperties]
public partial class
    IdentitySecurityLogToIdentitySecurityLogDtoMapper : MapperBase<IdentitySecurityLog, IdentitySecurityLogDto>
{
    [MapperIgnoreSource(nameof(IdentitySecurityLog.ConcurrencyStamp))]
    public override partial IdentitySecurityLogDto Map(IdentitySecurityLog source);

    [MapperIgnoreSource(nameof(IdentitySecurityLog.ConcurrencyStamp))]
    public override partial void Map(IdentitySecurityLog source, IdentitySecurityLogDto destination);
}