using Microsoft.EntityFrameworkCore;
using Vctoon.Comics;
using Vctoon.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;

namespace Vctoon.Libraries;

public static class LibraryDbContextModelBuilderExtensions
{
    public static void ConfigureLibraries(this
#nullable disable
        ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }
        
        
        builder.Entity<Library>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Libraries", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            // b.HasMany(x => x.Paths).WithOne().HasForeignKey(x => x.LibraryId).OnDelete(DeleteBehavior.Cascade);
            
            b.HasMany<Comic>().WithOne().HasForeignKey(x => x.LibraryId).OnDelete(DeleteBehavior.Cascade);
            
            /* Configure more properties here */
        });
        
        builder.Entity<LibraryPath>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "LibraryPaths", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasMany<ImageFile>().WithOne().HasForeignKey(x => x.LibraryPathId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany<ArchiveInfo>().WithOne().HasForeignKey(x => x.LibraryPathId).OnDelete(DeleteBehavior.Cascade);
            // b.HasMany(x => x.Children).WithOne().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });
        
        
        builder.Entity<Tag>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Tags", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.HasMany<Comic>().WithMany(x => x.Tags)
                .UsingEntity(j => j.ToTable(VctoonDbContext.EfCoreShadowTableName.ComicTags));
            /* Configure more properties here */
        });
        
        builder.Entity<ImageFile>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ImageFiles", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            /* Configure more properties here */
        });
        
        builder.Entity<ArchiveInfo>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ArchiveInfos", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.HasMany(x => x.Paths).WithOne().HasForeignKey(x => x.ArchiveInfoId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });
        
        
        builder.Entity<ArchiveInfoPath>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ArchiveInfoPaths", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            // b.HasMany(x => x.Children).WithOne().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany<ImageFile>().WithOne().HasForeignKey(x => x.ArchiveInfoPathId).OnDelete(DeleteBehavior.Cascade);
            
            /* Configure more properties here */
        });
        
        builder.Entity<ContentProgress>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ContentProgresses", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Comic>().WithMany(x => x.Progresses).HasForeignKey(x => x.ComicId).OnDelete(DeleteBehavior.Cascade);
            
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