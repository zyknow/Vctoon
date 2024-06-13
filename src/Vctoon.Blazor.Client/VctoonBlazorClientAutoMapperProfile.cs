using AutoMapper;
using Vctoon.Blazor.Client.Pages.Settings.UserManager;
using Vctoon.Identities.Dtos;

namespace Vctoon.Blazor.Client;

public class VctoonBlazorClientAutoMapperProfile : Profile
{
    public VctoonBlazorClientAutoMapperProfile()
    {
        CreateMap<IdentityUserExtraCreateOrUpdateModel, IdentityUserExtraCreateDto>();
        CreateMap<IdentityUserExtraDto, IdentityUserExtraCreateOrUpdateModel>();
        CreateMap<IdentityUserExtraCreateOrUpdateModel, IdentityUserExtraUpdateDto>();
    }
}