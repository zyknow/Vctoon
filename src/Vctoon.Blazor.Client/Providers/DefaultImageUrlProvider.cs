using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;

namespace Vctoon.Blazor.Client.Providers;

public class DefaultImageUrlProvider(
    IOptions<AbpRemoteServiceOptions> remoteServiceOptions)
    : IImageUrlProvider, ITransientDependency
{
    public string GetCoverUrl(string coverPath)
    {
        string baseUrl = remoteServiceOptions.Value.RemoteServices.Default.BaseUrl;
        string? url = $@"{baseUrl.TrimEnd('/')}/api/app/resource/cover?coverPath={coverPath}";
        return url;
    }

    public string GetImageUrl(Guid imageFileId, int? maxWidth = null)
    {
        if (maxWidth == null || maxWidth <= 0)
        {
            maxWidth = null;
        }

        string baseUrl = remoteServiceOptions.Value.RemoteServices.Default.BaseUrl;

        string? url =
            $@"{baseUrl.TrimEnd('/')}/api/app/resource/image/{imageFileId}?maxWidth={maxWidth}";

        return url;
    }
}