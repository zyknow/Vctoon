using System;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class ImageFileDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string Path { get; set; }

    public string Extension { get; set; }

    public uint Size { get; set; }

    public uint Width { get; set; }

    public uint Height { get; set; }

    public Guid? LibraryPathId { get; set; }

    public Guid? ArchiveInfoPathId { get; set; }

    public Guid ComicChapterId { get; set; }

    // public List<Tag> Tags { get; set; }
}