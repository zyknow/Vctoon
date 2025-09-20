using System.ComponentModel.DataAnnotations;
using Vctoon.Mediums;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryCreateUpdateDto
{
    [Required] public string Name { get; set; }

    public MediumType MediumType { get; set; }

    [MinLength(1)] public List<string> Paths { get; set; } = [];
}