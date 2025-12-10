using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vctoon.Mediums.Dtos;

public class MediumSeriesListUpdateDto
{
    [Required]
    public Guid SeriesId { get; set; }

    [Required]
    public List<Guid> MediumIds { get; set; } = new();
}
