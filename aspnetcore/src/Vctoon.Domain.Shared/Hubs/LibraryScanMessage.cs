using System;

namespace Vctoon.Hubs;

public class LibraryScanMessage
{
    public Guid LibraryId { get; set; }
    public string Title { get; set; }
    public string? Message { get; set; }
}