using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class ImageFileRepository : EfCoreRepository<VctoonDbContext, ImageFile, Guid>, IImageFileRepository
{
    public ImageFileRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<ImageFile>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}