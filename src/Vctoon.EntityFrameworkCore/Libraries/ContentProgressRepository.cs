using Vctoon.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Vctoon.Libraries;

public class ContentProgressRepository : EfCoreRepository<VctoonDbContext, ContentProgress, Guid>,
    IContentProgressRepository
{
    public ContentProgressRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}