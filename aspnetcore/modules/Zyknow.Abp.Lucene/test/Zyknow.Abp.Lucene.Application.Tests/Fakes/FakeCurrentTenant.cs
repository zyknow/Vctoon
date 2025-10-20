using Volo.Abp.MultiTenancy;

namespace Zyknow.Abp.Lucene.Application.Tests.Fakes;

public class FakeCurrentTenant : ICurrentTenant
{
    public Guid? ParentId => null;
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAvailable => Id.HasValue;

    public IDisposable Change(Guid? id, string? name = null)
    {
        return new NoopDisposable(() =>
        {
            Id = id;
            Name = name ?? string.Empty;
        });
    }

    private class NoopDisposable(Action onDispose) : IDisposable
    {
        public void Dispose()
        {
            onDispose();
        }
    }
}