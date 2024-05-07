using System.ComponentModel.DataAnnotations;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryCreateUpdateDto
{
    [Required] public string Name { get; set; }
    
    [MinLength(1)] public List<string> Paths { get; set; } = [];
}