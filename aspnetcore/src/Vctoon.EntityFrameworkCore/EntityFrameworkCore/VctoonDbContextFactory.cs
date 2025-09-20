using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Vctoon.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class VctoonDbContextFactory : IDesignTimeDbContextFactory<VctoonDbContext>
{
    public VctoonDbContext CreateDbContext(string[] args)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var configuration = BuildConfiguration();

        VctoonEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<VctoonDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new VctoonDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Vctoon.DbMigrator/"))
            .AddJsonFile("appsettings.json", false);

        return builder.Build();
    }
}