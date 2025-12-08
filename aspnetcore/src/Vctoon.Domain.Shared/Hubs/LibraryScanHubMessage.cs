using System;

namespace Vctoon.Hubs;

public class LibraryScanHubMessage
{
    public Guid LibraryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool Updated { get; set; }
}