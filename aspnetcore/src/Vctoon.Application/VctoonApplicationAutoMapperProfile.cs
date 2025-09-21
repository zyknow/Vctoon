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
        // Video -> DTOs，映射首个阅读进度到 Progress/LastReadTime（避免使用?.）
        CreateMap<Video, VideoDto>()
            .ForMember(d => d.Progress,
                opt => opt.MapFrom(s => s.Processes.Select(p => (double?)p.Progress).FirstOrDefault()))
            .ForMember(d => d.LastReadTime,
                opt => opt.MapFrom(s => s.Processes.Select(p => p.LastReadTime).FirstOrDefault()));
        CreateMap<Video, VideoGetListOutputDto>()
            .ForMember(d => d.Progress,
                opt => opt.MapFrom(s => s.Processes.Select(p => (double?)p.Progress).FirstOrDefault()))
            .ForMember(d => d.LastReadTime,
                opt => opt.MapFrom(s => s.Processes.Select(p => p.LastReadTime).FirstOrDefault()));
        CreateMap<VideoCreateUpdateDto, Video>(MemberList.Source);
        // Comic -> DTOs，映射首个阅读进度到 Progress/LastReadTime（避免使用?.）
        CreateMap<Comic, ComicDto>()
            .ForMember(d => d.Progress,
                opt => opt.MapFrom(s => s.Processes.Select(p => (double?)p.Progress).FirstOrDefault()))
            .ForMember(d => d.LastReadTime,
                opt => opt.MapFrom(s => s.Processes.Select(p => p.LastReadTime).FirstOrDefault()));
        CreateMap<Comic, ComicGetListOutputDto>()
            .ForMember(d => d.Progress,
                opt => opt.MapFrom(s => s.Processes.Select(p => (double?)p.Progress).FirstOrDefault()))
            .ForMember(d => d.LastReadTime,
                opt => opt.MapFrom(s => s.Processes.Select(p => p.LastReadTime).FirstOrDefault()));
        CreateMap<ComicCreateUpdateDto, Comic>(MemberList.Source);
        CreateMap<Artist, ArtistDto>();
        CreateMap<ArtistCreateUpdateDto, Artist>(MemberList.Source);
    }
}