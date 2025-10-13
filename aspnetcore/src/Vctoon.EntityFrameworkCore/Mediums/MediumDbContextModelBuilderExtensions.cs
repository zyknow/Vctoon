using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Vctoon.Mediums;

public static class MediumDbContextModelBuilderExtensions
{
    public static void ConfigureMediums(this
#nullable disable
        ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }


        builder.Entity<Video>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Videos", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.Processes).WithOne().HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });


        builder.Entity<Comic>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Comics", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.Processes).WithOne().HasForeignKey(x => x.ComicId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });
    }
}