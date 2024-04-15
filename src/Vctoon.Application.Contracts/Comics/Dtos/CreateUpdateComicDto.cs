using System;

namespace Vctoon.Comics.Dtos;

[Serializable]
public class CreateUpdateComicDto
{
    public string Title { get; set; }

    public string CoverPath { get; set; }
}