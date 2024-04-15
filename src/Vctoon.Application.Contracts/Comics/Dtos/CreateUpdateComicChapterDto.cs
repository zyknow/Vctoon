using System;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class CreateUpdateComicChapterDto
{
    public string CoverPath { get; set; }

    public string Title { get; set; }

    public uint PageCount { get; set; }

    public uint Size { get; set; }

    // public LibraryPathDto LibraryPath { get; set; }
}