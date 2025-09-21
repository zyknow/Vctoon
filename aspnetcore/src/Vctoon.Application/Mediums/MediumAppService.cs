using Vctoon.BlobContainers;
using Volo.Abp.BlobStoring;

namespace Vctoon.Mediums;

public class MediumAppService(IBlobContainer<CoverContainer> coverContainer) : ApplicationService
{
#if !DEBUG
    [Authorize]
#endif
    public async Task<Stream> GetCoverAsync(string cover)
    {
        var stream = await coverContainer.GetAsync(cover);
        return stream;
    }
}