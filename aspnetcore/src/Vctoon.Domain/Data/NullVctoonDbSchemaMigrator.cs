using Volo.Abp.DependencyInjection;

namespace Vctoon.Data;

/* This is used if database provider does't define
 * IVctoonDbSchemaMigrator implementation.
 */
public class NullVctoonDbSchemaMigrator : IVctoonDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}