namespace Vctoon.Libraries;

public interface IArtistAppService :
    ICrudAppService<
        ArtistDto,
        Guid,
        ArtistGetListInput,
        ArtistCreateUpdateDto,
        ArtistCreateUpdateDto>
{
}