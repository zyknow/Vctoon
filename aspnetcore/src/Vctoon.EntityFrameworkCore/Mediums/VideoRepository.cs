using Vctoon.Mediums.Base;

namespace Vctoon.Mediums;

public class VideoRepository : MediumBaseRepository<Video>, IVideoRepository
{
    public VideoRepository(IDbContextProvider<VctoonDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Video>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}