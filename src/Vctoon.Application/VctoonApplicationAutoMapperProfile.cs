using AutoMapper;
using Vctoon.Comics;
using Vctoon.Comics.Dtos;
using Vctoon.Libraries;
using Vctoon.Libraries.Dtos;
using Volo.Abp.AutoMapper;

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
                opt.MapFrom(x => x.Paths.Select(x => x.Path)
                ))
            ;

        CreateMap<LibraryCreateUpdateDto, Library>(MemberList.Source).Ignore(x => x.Paths);
        CreateMap<Comic, ComicDto>();
        CreateMap<ComicCreateUpdateDto, Comic>(MemberList.Source);
        CreateMap<ComicChapter, ComicChapterDto>();
        CreateMap<ComicChapterCreateUpdateDto, ComicChapter>(MemberList.Source);
        CreateMap<Tag, TagDto>();
        CreateMap<TagCreateUpdateDto, Tag>(MemberList.Source);
        CreateMap<TagGroup, TagGroupDto>();
        CreateMap<TagGroupCreateUpdateDto, TagGroup>(MemberList.Source);
        CreateMap<ImageFile, ImageFileDto>();
        CreateMap<ImageFileCreateUpdateDto, ImageFile>(MemberList.Source);
    }
}