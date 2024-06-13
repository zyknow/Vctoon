using System.ComponentModel.DataAnnotations;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class TagCreateUpdateDto
{
    [Required] [MinLength(1)] public string Name { get; set; }
}