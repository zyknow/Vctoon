using Microsoft.EntityFrameworkCore;
using Vctoon.Libraries;
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
        
        builder.Entity<IdentityUserLibraryPermissionGrant>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "IdentityUserLibraryPermissionGrants", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.HasMany(x => x.LibraryPermissions).WithOne().HasForeignKey(x => x.IdentityUserLibraryPermissionGrantId)
                .OnDelete(DeleteBehavior.Cascade);
            
            b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            
            /* Configure more properties here */
        });
        
        builder.Entity<LibraryPermission>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "LibraryPermissions", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.HasOne<Library>().WithMany().HasForeignKey(x => x.LibraryId).OnDelete(DeleteBehavior.Cascade);
            
            /* Configure more properties here */
        });
    }
}