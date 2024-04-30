using System.ComponentModel.DataAnnotations;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryCreateUpdateDto
{
    [Required]
    [MinLength(10)]
    public string Name { get; set; }

    public List<string> Paths { get; set; } = [];
}