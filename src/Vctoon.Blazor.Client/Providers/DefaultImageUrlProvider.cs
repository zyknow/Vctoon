using Volo.Abp.DependencyInjection;

namespace Vctoon.Blazor.Client.Providers;

public class DefaultImageUrlProvider(IBlazorHttpRequestAccessor blazorHttpRequestAccessor)
    : IImageUrlProvider, ITransientDependency
{
    public string GetCoverUrl(string coverPath)
    {
        string? url = $@"{blazorHttpRequestAccessor.RemoteUrl.TrimEnd('/')}/api/app/image/cover?coverPath={coverPath}";
        return url;
    }
    
    public string GetImageUrl(Guid imageFileId, int? maxWidth = null)
    {
        if (maxWidth == null || maxWidth <= 0)
        {
            maxWidth = null;
        }
        
        string? url =
            $@"{blazorHttpRequestAccessor.RemoteUrl.TrimEnd('/')}/api/app/image/image/{imageFileId}?maxWidth={maxWidth}";
        
        return url;
    }
}