using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public class VideoRepository(IDbContextProvider<VctoonDbContext> dbContextProvider)
    : MediumBaseRepository<Video>(dbContextProvider), IVideoRepository
{
    public override async Task<IQueryable<Video>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}