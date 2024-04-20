namespace Vctoon.Data;

public interface IVctoonDbSchemaMigrator
{
    Task MigrateAsync();
}