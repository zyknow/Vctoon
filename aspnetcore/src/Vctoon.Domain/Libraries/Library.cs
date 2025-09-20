using Vctoon.Mediums;

namespace Vctoon.Libraries;

public class Library : AuditedEntity<Guid>
{
    protected Library()
    {
    }

    internal Library(
        Guid id,
        string name,
        MediumType mediumType
    ) : base(id)
    {
        SetName(name);
        MediumType = mediumType;
    }

    public string Name { get; protected set; }

    public virtual List<LibraryPath> Paths { get; internal set; } = new();
    public MediumType MediumType { get; set; }

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public void AddPath(LibraryPath libraryPath)
    {
        if (Paths.Any(x => x.Path == libraryPath.Path || x.Id == libraryPath.Id))
        {
            return;
        }

        Paths.Add(libraryPath);
    }

    public void AddPaths(List<LibraryPath> libraryPaths)
    {
        var addPaths = libraryPaths.DistinctBy(x => x.Path).ToList();
        foreach (var libraryPath in addPaths)
        {
            AddPath(libraryPath);
        }
    }

    public void RemovePath(string path)
    {
        var libraryPath = Paths.FirstOrDefault(x => x.Path == path);
        if (libraryPath == null)
        {
            return;
        }

        Paths.Remove(libraryPath);
    }

    public void RemovePaths(List<string> paths)
    {
        foreach (var path in paths)
        {
            RemovePath(path);
        }
    }
}