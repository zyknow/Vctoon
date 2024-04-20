using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Vctoon.Comics;

public static class ComicDbContextModelBuilderExtensions
{
    public static void ConfigureComics(this
#nullable disable
        ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        if (builder.IsTenantOnlyDatabase())
            return;

        builder.Entity<Comic>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "Comics", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.Chapters).WithOne().HasForeignKey(x => x.ComicId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });


        builder.Entity<ComicChapter>(b =>
        {
            b.ToTable(VctoonConsts.DbTablePrefix + "ComicChapters", VctoonConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasMany(x => x.Images).WithOne().HasForeignKey(x => x.ComicChapterId).OnDelete(DeleteBehavior.Cascade);
            /* Configure more properties here */
        });
    }
}