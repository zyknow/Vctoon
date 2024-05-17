using Volo.Abp.Application.Dtos;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class ComicChapterDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; }
    
    public string CoverPath { get; set; }
    
    public uint PageCount { get; set; }
    
    public uint Size { get; set; }
    
    public Guid ComicId { get; set; }
    
    public double Progress { get; set; }
    
    // public List<ImageFile> Images { get; set; }
    //
    // public List<Tag> Tags { get; set; }
}