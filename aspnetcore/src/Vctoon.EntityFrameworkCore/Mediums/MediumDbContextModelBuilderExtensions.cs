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

        builder.Entity<Medium>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Mediums", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.Processes).WithOne().HasForeignKey(x => x.MediumId).OnDelete(DeleteBehavior.Cascade);

            b.OwnsOne(x => x.VideoDetail, navigationBuilder =>
            {
                navigationBuilder.ToJson();
            });
        });

        builder.Entity<MediumSeriesLink>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "MediumSeriesLinks", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(x => new { x.SeriesId, x.MediumId }).IsUnique();
            b.HasIndex(x => new { x.SeriesId, x.Sort });

            b.HasOne(x => x.Series)
                .WithMany(x => x.ItemLinks)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Medium)
                .WithMany(x => x.SeriesLinks)
                .HasForeignKey(x => x.MediumId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}