using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Vctoon.Identities;

public static class IdentityUserLibraryPermissionDbContextModelBuilderExtensions
{
    public static void ConfigureIdentityUserLibraryPermissions(this
#nullable disable
        ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<IdentityUserExtra>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "IdentityUserExtras", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasOne(x => x.User).WithOne().HasForeignKey<IdentityUserExtra>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

            /* Configure more properties here */
        });
    }
}