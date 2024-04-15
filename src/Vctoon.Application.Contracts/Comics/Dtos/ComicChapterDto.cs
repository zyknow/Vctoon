using System;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicChapterDto : EntityDto<Guid>
{
    public string CoverPath { get; set; }

    public string Title { get; set; }

    public Guid ComicId { get; set; }

    public ComicDto Comic { get; set; }

    public uint PageCount { get; set; }

    public uint Size { get; set; }

    // public LibraryPathDto LibraryPath { get; set; }
}