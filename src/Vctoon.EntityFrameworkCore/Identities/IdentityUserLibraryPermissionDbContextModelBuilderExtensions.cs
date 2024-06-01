using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;

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
            
            b.HasMany(x => x.LibraryPermissions).WithOne().HasForeignKey(x => x.IdentityUserExtraId)
                .OnDelete(DeleteBehavior.Cascade);
            
            b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            
            /* Configure more properties here */
        });
    }
}