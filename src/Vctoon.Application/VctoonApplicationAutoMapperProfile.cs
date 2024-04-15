using Vctoon.Libraries;
using Vctoon.Libraries.Dtos;
using Vctoon.Comics;
using Vctoon.Comics.Dtos;
using AutoMapper;

namespace Vctoon;

public class VctoonApplicationAutoMapperProfile : Profile
{
    public VctoonApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<Library, LibraryDto>();
        CreateMap<CreateUpdateLibraryDto, Library>(MemberList.Source);
        CreateMap<ComicChapter, ComicChapterDto>();
        CreateMap<CreateUpdateComicChapterDto, ComicChapter>(MemberList.Source);
        CreateMap<Comic, ComicDto>();
        CreateMap<CreateUpdateComicDto, Comic>(MemberList.Source);
        CreateMap<Favorite, LibraryCollectionDto>();
        CreateMap<CreateUpdateLibraryCollectionDto, Favorite>(MemberList.Source);
        CreateMap<Tag, TagDto>();
        CreateMap<CreateUpdateTagDto, Tag>(MemberList.Source);
    }
}