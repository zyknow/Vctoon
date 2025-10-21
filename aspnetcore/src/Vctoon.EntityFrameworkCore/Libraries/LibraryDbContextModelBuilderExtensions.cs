using Vctoon.Mediums;
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

            b.HasMany(x => x.Paths).WithOne().HasForeignKey(x => x.LibraryId).OnDelete(DeleteBehavior.Cascade);
            
            // 级联删除Zyknow.Abp.Lucene 会导致更新Lucene索引
            b.HasMany<Comic>().WithOne().HasForeignKey(x => x.LibraryId);
            b.HasMany<Video>().WithOne().HasForeignKey(x => x.LibraryId);

            /* Configure more properties here */
        });

        builder.Entity<LibraryPath>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "LibraryPaths", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasMany<ComicImage>().WithOne().HasForeignKey(x => x.LibraryPathId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany<ArchiveInfo>().WithOne().HasForeignKey(x => x.LibraryPathId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany<Comic>().WithOne().HasForeignKey(x => x.LibraryPathId);
            b.HasMany<Video>().WithOne().HasForeignKey(x => x.LibraryPathId);
            // b.HasMany(x => x.Children).WithOne().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });


        builder.Entity<Tag>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Tags", VctoonConsts.DbSchema);
            b.HasIndex(x => x.Name).IsUnique();
            b.ConfigureByConvention();
        });

        builder.Entity<ComicImage>(b =>
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
            b.HasMany<ComicImage>().WithOne().HasForeignKey(x => x.ArchiveInfoPathId).OnDelete(DeleteBehavior.Cascade);

            /* Configure more properties here */
        });

        builder.Entity<LibraryPermission>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "LibraryPermissions", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasKey(x => new { x.LibraryId, x.UserId });
            b.HasOne<Library>().WithMany().HasForeignKey(x => x.LibraryId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });

        builder.Entity<Artist>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Artists", VctoonConsts.DbSchema);
            b.HasIndex(x => x.Name).IsUnique();
            b.ConfigureByConvention();
            /* Configure more properties here */
        });
    }
}