using System;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicCreateUpdateDto
{
    public string Title { get; set; }

    public string CoverPath { get; set; }

    public Guid LibraryId { get; set; }

    // public List<Tag> Tags { get; set; }
    //
    // public List<ComicChapter> Chapters { get; set; }
}