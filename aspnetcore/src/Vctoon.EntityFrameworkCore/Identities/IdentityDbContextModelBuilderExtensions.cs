using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Vctoon.Identities;

public static class IdentityDbContextModelBuilderExtensions
{
    public static void ConfigureIdentities(this
#nullable disable
        ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<IdentityUserReadingProcess>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "IdentityUserReadingProcesses", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
        });
    }
}