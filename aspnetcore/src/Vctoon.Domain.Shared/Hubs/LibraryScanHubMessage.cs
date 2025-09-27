using System;

namespace Vctoon.Hubs;

public class LibraryScanHubMessage
{
    public Guid LibraryId { get; set; }
    public string Title { get; set; }
    public string? Message { get; set; }
}