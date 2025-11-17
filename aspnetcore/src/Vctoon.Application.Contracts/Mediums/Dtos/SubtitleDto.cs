namespace Vctoon.Mediums.Dtos;

[Serializable]
public class SubtitleDto
{
    public string FileName { get; set; } = default!;

    public string? Language { get; set; }

    public string? Label { get; set; }

    public string Format { get; set; } = default!;

    public string Url { get; set; } = default!;
}
