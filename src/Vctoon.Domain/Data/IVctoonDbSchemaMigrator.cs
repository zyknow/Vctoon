using System.Threading.Tasks;

namespace Vctoon.Data;

public interface IVctoonDbSchemaMigrator
{
    Task MigrateAsync();
}