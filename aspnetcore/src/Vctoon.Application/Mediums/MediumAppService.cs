using Vctoon.BlobContainers;
using Vctoon.Identities;
using Vctoon.ImageProviders;
using Vctoon.Mediums.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Mediums;

public partial class MediumAppService(
    IArchiveInfoRepository archiveInfoRepository,
    IImageProvider imageProvider,
    IMediumRepository mediumRepository,
    IRepository<ComicImage, Guid> comicImageRepository,
    IBlobContainer<CoverContainer> coverContainer,
    CoverSaver coverSaver,
    IIdentityUserReadingProcessRepository readingProcessRepository,
    IRepository<Artist, Guid> artistRepository,
    ITagRepository tagRepository,
    IRepository<MediumSeriesLink, Guid> mediumSeriesLinkRepository) : VctoonCrudAppService<
    Medium,
    MediumDto,
    Guid,
    MediumGetListInput,
    CreateUpdateMediumDto, CreateUpdateMediumDto>(mediumRepository), IMediumAppService
{

}