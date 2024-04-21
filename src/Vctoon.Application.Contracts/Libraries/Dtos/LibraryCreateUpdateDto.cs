namespace Vctoon.Libraries.Dtos;

[Serializable]
public class LibraryCreateUpdateDto
{
    public string Name { get; set; }

    public List<string> Paths { get; set; } = [];
}