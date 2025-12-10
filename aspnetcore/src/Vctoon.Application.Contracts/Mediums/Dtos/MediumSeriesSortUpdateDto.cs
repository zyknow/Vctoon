using System.ComponentModel.DataAnnotations;

namespace Vctoon.Mediums.Dtos;

public class MediumSeriesSortUpdateDto
{
    [Required]
    public Guid SeriesId { get; set; }

    [Required]
    public List<Guid> MediumIds { get; set; } = new();
}
