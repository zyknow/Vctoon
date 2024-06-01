using Microsoft.EntityFrameworkCore;
using Vctoon.Comics;
using Vctoon.Identities;
using Vctoon.Libraries;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

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
    
    // Vctoon
    public DbSet<Library> Libraries { get; set; }
    public DbSet<Comic> Comics { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ImageFile> ImageFiles { get; set; }
    public DbSet<ArchiveInfo> ArchiveInfos { get; set; }
    public DbSet<ArchiveInfoPath> ArchiveInfoPaths { get; set; }
    public DbSet<LibraryPath> LibraryPaths { get; set; }
    public DbSet<ContentProgress> ContentProgresses { get; set; }
    
    public DbSet<LibraryPermission> LibraryPermissions { get; set; }
    public DbSet<IdentityUserExtra> IdentityUserExtras { get; set; }
    
    
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
        
        builder.ConfigureComics();
        builder.ConfigureLibraries();
        builder.ConfigureIdentityUserLibraryPermissions();
        
        /* Configure your own tables/entities inside here */
        
        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(VctoonConsts.DbTablePrefix + "YourEntities", VctoonConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
        
        
        builder.Entity<LibraryPermission>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "LibraryPermissions", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            
            /* Configure more properties here */
        });
        
        
        builder.Entity<IdentityUserExtra>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "IdentityUserExtras", VctoonConsts.DbSchema);
            b.ConfigureByConvention();
            
            
            /* Configure more properties here */
        });
    }
    
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    
    public class EfCoreShadowTableName
    {
        public const string TagGroupTags = $"{VctoonConsts.DbTablePrefix}TagGroupTags";
        public const string ImageFileTags = $"{VctoonConsts.DbTablePrefix}ImageFileTags";
        public const string ComicTags = $"{VctoonConsts.DbTablePrefix}ComicTags";
    }
    
    
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
    
    #endregion
}