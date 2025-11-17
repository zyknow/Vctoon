namespace Vctoon.Helper;

public class ConverterHelper
{
    public static string MapToRemoteContentType(string fileOrFileExtension)
    {
        
        return Path.GetExtension(fileOrFileExtension).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tiff" => "image/tiff",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".mkv" => "video/x-matroska",
            ".vtt" => "text/vtt",
            ".srt" => "text/plain",
            ".ass" => "text/plain",
            _ => "application/octet-stream", // Default content type for unknown extensions
        };
    }
}