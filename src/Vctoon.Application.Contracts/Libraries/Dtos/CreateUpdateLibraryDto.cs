using System;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class CreateUpdateLibraryDto
{
    public string Name { get; set; }

    // public List<LibraryPathDto> Paths { get; set; }
}