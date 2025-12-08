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
    }
}