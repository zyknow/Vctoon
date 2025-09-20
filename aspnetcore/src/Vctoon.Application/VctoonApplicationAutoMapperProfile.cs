using AutoMapper;
using Vctoon.Identities.Dtos;
using Vctoon.Libraries.Dtos;
using Vctoon.Mediums;
using Vctoon.Mediums.Dtos;
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
        CreateMap<Tag, TagDto>();
        CreateMap<TagCreateUpdateDto, Tag>(MemberList.Source);
        CreateMap<ComicImage, ComicImageDto>();
        CreateMap<ComicImage, ComicGetListOutputDto>();
        CreateMap<ComicImageCreateUpdateDto, ComicImage>(MemberList.Source);
        CreateMap<LibraryPermission, LibraryPermissionDto>();
        CreateMap<LibraryPermissionCreateUpdateDto, LibraryPermission>(MemberList.Source);

        CreateMap<IdentitySecurityLog, IdentitySecurityLogDto>();
        CreateMap<Video, VideoDto>();
        CreateMap<Video, VideoGetListOutputDto>();
        CreateMap<VideoCreateUpdateDto, Video>(MemberList.Source);
        CreateMap<Comic, ComicDto>();
        CreateMap<Comic, ComicGetListOutputDto>();
        CreateMap<ComicCreateUpdateDto, Comic>(MemberList.Source);
        CreateMap<Artist, ArtistDto>();
        CreateMap<ArtistCreateUpdateDto, Artist>(MemberList.Source);
    }
}