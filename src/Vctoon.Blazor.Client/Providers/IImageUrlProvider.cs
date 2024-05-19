namespace Vctoon.Blazor.Client.Providers;

public interface IImageUrlProvider
{
    string GetCoverUrl(string coverPath);
    string GetImageUrl(Guid imageFileId, int? maxWidth = null);
}