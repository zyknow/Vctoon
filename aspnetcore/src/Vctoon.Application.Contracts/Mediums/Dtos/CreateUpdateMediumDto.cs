namespace Vctoon.Mediums.Dtos;

public class CreateUpdateMediumDto
{
    public string Title { get; set; }
    public MediumType MediumType { get; set; }
    public string Description { get; set; }
    public string Cover { get; set; }
    public Guid LibraryId { get; set; }
    public Guid? LibraryPathId { get; set; }
    public VideoDetail? VideoDetail { get; set; }
}
