using AutoMapper;
using Vctoon.Comics;
using Vctoon.Comics.Dtos;
using Vctoon.Identities.Dtos;
using Vctoon.Libraries;
using Vctoon.Libraries.Dtos;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;

namespace Vctoon;

public class VctoonApplicationAutoMapperProfile : Profile
{
    public VctoonApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */


        CreateMap<Library, LibraryDto>()
            .ForMember(x => x.Paths, opt =>
                opt.MapFrom(x => x.Paths.Select(p => p.Path)
                ))
            ;

        CreateMap<LibraryCreateUpdateDto, Library>(MemberList.Source).Ignore(x => x.Paths);
        CreateMap<Comic, ComicDto>()
            .ForMember(
                dest => dest.Progress,
                opt => opt.MapFrom(src =>
                    !src.Progresses.IsNullOrEmpty() ? src.Progresses.First().CompletionRate : null)
            );
        CreateMap<ComicCreateUpdateDto, Comic>(MemberList.Source);
        CreateMap<Tag, TagDto>();
        CreateMap<TagCreateUpdateDto, Tag>(MemberList.Source);
        CreateMap<ImageFile, ImageFileDto>();
        CreateMap<ImageFileCreateUpdateDto, ImageFile>(MemberList.Source);
        CreateMap<LibraryPermission, LibraryPermissionDto>();
        CreateMap<LibraryPermissionCreateUpdateDto, LibraryPermission>(MemberList.Source);
        CreateMap<LibraryPermissionCacheItem, LibraryPermissionDto>();

        CreateMap<IdentityUserDto, IdentityUserExtraDto>();
        CreateMap<IdentityUser, IdentityUserExtraDto>();
        CreateMap<IdentityUserExtraCreateDto, IdentityUser>(MemberList.Source);
        CreateMap<IdentityUserExtraUpdateDto, IdentityUser>(MemberList.Source);

        CreateMap<IdentitySecurityLog, IdentitySecurityLogDto>();
    }
}