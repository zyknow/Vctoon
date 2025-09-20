using Microsoft.Extensions.DependencyInjection;
using Vctoon.Data;
using Volo.Abp.DependencyInjection;

namespace Vctoon.EntityFrameworkCore;

public class EntityFrameworkCoreVctoonDbSchemaMigrator
    : IVctoonDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreVctoonDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the VctoonDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<VctoonDbContext>()
            .Database
            .MigrateAsync();
    }
}