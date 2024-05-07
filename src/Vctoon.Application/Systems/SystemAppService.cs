using Microsoft.AspNetCore.Authorization;

namespace Vctoon.Systems;

public class SystemAppService : VctoonAppService, ISystemAppService
{
    [Authorize(Policy = VctoonPermissions.Library.Create)]
    public async Task<List<string>> GetSystemPathsAsync(string? path = null)
    {
        if (path.IsNullOrEmpty())
        {
            return DriveInfo.GetDrives()
                .Select(x => x.Name)
                .ToList();
        }
        
        DirectoryInfo dirs = new DirectoryInfo(path);
        List<string> accessibleDirs = new List<string>();
        
        foreach (DirectoryInfo dir in dirs.GetDirectories())
        {
            try
            {
                // Try to get a directory listing to see if we have access
                dir.GetFileSystemInfos();
                accessibleDirs.Add(dir.FullName);
            }
            catch (UnauthorizedAccessException)
            {
                // If we can't access the directory, skip it
            }
        }
        
        return accessibleDirs;
    }
}