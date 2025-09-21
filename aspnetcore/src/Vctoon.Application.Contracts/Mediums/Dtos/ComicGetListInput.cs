namespace Vctoon.Mediums.Dtos;

[Serializable]
public class ComicGetListInput : MediumGetListInputBase, IMediumHasReadingProcessQuery
{
    public bool Progressing { get; set; }
}