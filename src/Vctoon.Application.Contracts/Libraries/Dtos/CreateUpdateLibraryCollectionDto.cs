using System;

namespace Vctoon.Libraries.Dtos;

[Serializable]
public class CreateUpdateLibraryCollectionDto
{
    public string Name { get; set; }

    public string CoverPath { get; set; }
}