using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Vctoon.Libraries;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Vctoon.Comics;

namespace Vctoon.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class VctoonDbContext :
    AbpDbContext<VctoonDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    public VctoonDbContext(DbContextOptions<VctoonDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(VctoonConsts.DbTablePrefix + "YourEntities", VctoonConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});


        builder.Entity<Library>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Libraries", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany<Comic>().WithOne(x => x.Library).HasForeignKey(x => x.LibraryId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Paths).WithOne(x => x.Library).HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            /* Configure more properties here */
        });

        builder.Entity<LibraryPath>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "LibraryPaths", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.Children).WithOne(x => x.Parent).HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany<ComicChapter>().WithOne(x => x.LibraryPath).HasForeignKey(x => x.LibraryPathId)
                .OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });

        builder.Entity<Comic>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Comics", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.Chapters).WithOne(x => x.Comic).HasForeignKey(x => x.ComicId)
                .OnDelete(DeleteBehavior.Cascade);

            // b.HasMany<ComicFavorite>().WithOne(x => x.Comic).HasForeignKey(x => x.ComicId)
            //     .OnDelete(DeleteBehavior.Cascade);

            /* Configure more properties here */
        });


        builder.Entity<ComicChapter>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ComicChapters", VctoonConsts.DbSchema);
            b.ConfigureByConvention();


            // b.HasMany<ComicChapterFavorite>().WithOne(x => x.ComicChapter).HasForeignKey(x => x.ComicChapterId)
            //     .OnDelete(DeleteBehavior.Cascade);

            /* Configure more properties here */
        });


        builder.Entity<Favorite>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Favorites", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.ComicFavorites).WithOne(x => x.Favorite).HasForeignKey(x => x.FavoriteId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.ComicChapterFavorites).WithOne(x => x.Favorite).HasForeignKey(x => x.FavoriteId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.ComicFileFavorites).WithOne(x => x.Favorite).HasForeignKey(x => x.FavoriteId)
                .OnDelete(DeleteBehavior.Cascade);

            /* Configure more properties here */
        });


        builder.Entity<Tag>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Tags", VctoonConsts.DbSchema);
            b.ConfigureByConvention();


            /* Configure more properties here */
        });


        builder.Entity<ComicFileFavorite>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ComicFileFavorites", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            /* Configure more properties here */
        });

        builder.Entity<ComicChapterFavorite>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ComicChapterFavorites", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasOne(x => x.ComicChapter).WithMany().HasForeignKey(x => x.ComicChapterId);
            /* Configure more properties here */
        });

        builder.Entity<ComicFavorite>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ComicFavorites", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasOne(x => x.Comic).WithMany().HasForeignKey(x => x.ComicId);
            /* Configure more properties here */
        });
    }
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public DbSet<Library> Libraries { get; set; }
    public DbSet<ComicChapter> ComicChapters { get; set; }
    public DbSet<Comic> Comics { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Tag> Tags { get; set; }

    #endregion
}