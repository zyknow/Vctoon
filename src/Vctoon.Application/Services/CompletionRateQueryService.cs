using Volo.Abp.DependencyInjection;

namespace Vctoon.Services;

public class CompletionRateQueryService() : VctoonService, ITransientDependency
{
    public async Task<double> GetCompletionRateAsync(Guid comicId)
    {
    }
}